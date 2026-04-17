# xaml-lint — Design

| | |
|---|---|
| **Status** | Draft |
| **Date** | 2026-04-17 |
| **Author** | Jan Ivar Z. Carlsen |
| **Supersedes** | — |

## 1. Overview

`xaml-lint` is a Claude Code plugin that lints XAML files for common issues.

**Primary consumer:** Claude itself. A `PostToolUse` hook on `Write`/`Edit` tool calls for `*.xaml` files fires `xaml-lint` automatically; diagnostics land back in Claude's context so it can act on them without prompting.

**Secondary consumers:** humans typing `/xaml-lint:lint` in a session, and CI pipelines consuming SARIF.

**v1 goal:** port the 13-rule portable shortlist from the Rapid XAML Toolkit, covering WPF (primary) and UWP/WinUI 3/MAUI where upstream rules apply. All plumbing polished: tests, docs, `dotnet tool` publish, marketplace-ready plugin manifest.

**Non-goals for v1:**

- Auto-fix (diagnostics only; Claude applies fixes via its existing `Edit` tool)
- IDE-surface code (light bulbs, quick actions, refactorings)
- Code generation from view models
- LSP server (deferred to v2 — see §12)
- SARIF upload integrations (users wire their own CI)

**Attribution:** analysis rules are derived from [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) (Matt Lacey, MIT). Each ported rule carries upstream credit in its file header and an `upstreamId` field in rule metadata. `LICENSE` preserves Matt Lacey's copyright line.

## 2. Architecture

Three layers, bottom-up.

### 2.1 Rule engine (`XamlLint.Core`)

A standalone .NET class library with no I/O, no process concerns, no global state. Pure `Input → Diagnostics[]`. Every rule is a stateless class.

Stateless-by-design is an explicit requirement so that v2's LSP server is purely additive — wrap the engine, don't rewrite it.

### 2.2 CLI (`XamlLint.Cli`)

The `dotnet tool install -g xaml-lint` entry point. Responsibilities:

- Parse args (`System.CommandLine` current stable).
- Resolve config via walk-up discovery.
- Resolve dialect per file via cascade.
- Load each file as `XamlDocument`. Emit `LX001` on XML parse failure and short-circuit that file.
- Parse suppression pragmas into a `SuppressionMap` (pre-pass).
- Dispatch applicable rules in parallel per file (bounded `AsParallel`). Filter suppressed diagnostics.
- Format output. Emit to stdout (or `--output` file).
- Exit with correct code.

### 2.3 Plugin veneer

Thin wrapper making Claude reach for the CLI naturally.

- **`PostToolUse` hook** — matches `Write|Edit`; hook invokes `xaml-lint hook` (a CLI subcommand that reads Claude's hook JSON from stdin and dispatches to the lint pipeline). Keeping the hook as a CLI subcommand avoids shipping a separate bash/PowerShell shim.
- **Skill** (`skills/lint-xaml/SKILL.md`) — description triggers on "check my XAML", "lint this view", etc. Body: "Run `xaml-lint <file>` and interpret the JSON output."
- **Slash command** (`commands/lint.md`) — `/xaml-lint:lint <path-or-glob>` for explicit user invocation.
- **Plugin manifest** (`.claude-plugin/plugin.json`) — name, version, description, author, license, homepage, requirements note.

### 2.4 Install flow

1. User installs plugin via Claude Code plugin marketplace (or `claude --plugin-dir` for dev).
2. Plugin's install-time message (README + skill body) instructs: `dotnet tool install -g xaml-lint`.
3. Hook fires on next XAML edit.

Automatic `dotnet tool install` is **not** attempted in v1 — manual install with good docs. Revisit post-v1 once Anthropic's support for plugin-managed installs is understood.

## 3. Rule engine internals

### 3.1 Core types

```csharp
public interface IXamlRule
{
    RuleMetadata Metadata { get; }
    IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context);
}

public sealed record Diagnostic(
    string RuleId,
    Severity Severity,
    string Message,
    string File,
    int StartLine,
    int StartCol,
    int EndLine,
    int EndCol,
    string? HelpUri);

public enum Severity { Info, Warning, Error }

[Flags]
public enum Dialect
{
    None     = 0,
    Wpf      = 1,
    WinUI3   = 2,
    Uwp      = 4,
    Maui     = 8,
    Avalonia = 16,
    Uno      = 32,
    All      = Wpf | WinUI3 | Uwp | Maui | Avalonia | Uno
}
```

`XamlDocument` wraps `XDocument` loaded with `LoadOptions.SetLineInfo`, also carries the raw source text (for byte-exact spans the XDocument API loses) and the detected `Dialect`.

`RuleContext` carries active dialect, effective severity map (post-config), the `SuppressionMap`, and raw source access.

### 3.2 Rule declaration and discovery

Each rule class carries a `[XamlRule]` attribute that fully declares its metadata:

```csharp
[XamlRule(
    Id = "LX100",
    UpstreamId = "RXT101",
    Title = "Grid.Row used without matching RowDefinition",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX100.md")]
public sealed class GridRowWithoutRowDefinition : IXamlRule
{
    // Metadata property is generated from the attribute by the source generator;
    // rule authors only implement Analyze().
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) { ... }
}
```

Discovery is **source-generated at build time**. A generator project (`XamlLint.Core.SourceGen`, targeting `netstandard2.0` per Roslyn source-generator constraints) scans for `[XamlRule]`-annotated classes in the rules assembly and emits:

- A static `GeneratedRuleCatalog.Rules` list of all rules.
- A `Metadata` property implementation on each rule class that returns a `RuleMetadata` value populated from the attribute.
- A stub `docs/rules/LX###.md` file for any rule missing one, using the canonical 4-heading template (see §11). The stub is committed; the generator refuses to overwrite an existing doc.

Runtime has zero reflection and zero MEF. AOT-friendly.

The generator project sets `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` so generated catalog code lands on disk (debugging + meta-test inspection).

### 3.3 Dialect gating

Engine filters before invocation:

```csharp
var applicable = catalog.Where(r => (r.Metadata.Dialects & context.Dialect) != 0);
```

Rules also guard internally as a sanity belt.

### 3.4 Suppression pipeline (ReSharper-style)

Pragma grammar inside XAML comments:

- `<!-- xaml-lint disable once RULE [RULE...] -->` — skip next `XElement` only
- `<!-- xaml-lint disable RULE [RULE...] -->` ... `<!-- xaml-lint restore RULE [RULE...] -->` — block
- `<!-- xaml-lint disable RULE [RULE...] -->` with no matching `restore` — extends to end of file
- `<!-- xaml-lint disable All -->` or `<!-- xaml-lint disable -->` — all rules
- Multiple rule IDs space-separated
- Directive keywords (`disable`, `once`, `restore`, `All`) are case-sensitive

**"disable once" resolution:** the "next element" is the first `XElement` node following the comment in document order that isn't itself a pragma comment. Its span defines the suppressed range. Handles multi-line elements cleanly.

**Malformed pragma:** any comment whose body starts with the `xaml-lint` token but fails to parse as a valid directive emits `LX002` warning (location = comment's span). Never silent.

**Grammar (informal):**

```
pragma     := "xaml-lint" WS directive (WS target-list)?
directive  := "disable" (WS "once")? | "restore"
target-list := "All" | rule-id (WS rule-id)*
rule-id    := /[A-Z]+\d+/
```

Whitespace is flexible; keywords are case-sensitive; `once` only attaches to `disable` (never `restore`).

**Pipeline:**

1. Walk all `XComment` nodes in the document, in document order.
2. For each comment whose body begins with the `xaml-lint` token, parse against the grammar. Valid → extend `SuppressionMap`. Invalid → emit `LX002`.
3. `SuppressionMap` is keyed by rule ID (or `*` for `All`) and holds a list of `(startLine, endLine)` suppressed ranges per key.
4. After rules emit diagnostics, filter: drop any diagnostic whose `(ruleId, startLine)` falls inside a suppressed range for that rule (or `*`).

### 3.5 Rule IDs (v1)

Rule IDs are organized into **category ranges** (modeled on StyleCop's `SA####` scheme). 3-digit IDs, hundreds place indicates category. Keeps related rules clustered and gives ~100 IDs of headroom per category.

| Range | Category |
|---|---|
| LX001–LX099 | Tool / engine diagnostics |
| LX100–LX199 | Layout (Grid, StackPanel, DockPanel, Canvas, …) |
| LX200–LX299 | Bindings / data |
| LX300–LX399 | Naming |
| LX400–LX499 | Resources / localization |
| LX500–LX599 | Input / controls |
| LX600–LX699 | Deprecated patterns |
| LX700–LX899 | Reserved for future categories |
| LX900–LX999 | Reserved for opinionated / non-default rules (StyleCop's `SX` precedent) |

**v1 catalog** (19 IDs: 6 tool diagnostics + 13 lint rules):

| ID | Upstream | Title | Dialects | Default |
|---|---|---|---|---|
| LX001 | RXT999 | Malformed XAML | All | Error |
| LX002 | — | Unrecognized pragma directive | All | Warning |
| LX003 | — | Malformed configuration | All | Error |
| LX004 | — | Cannot read file | All | Error |
| LX005 | — | Skipping non-XAML file | All | Info |
| LX006 | — | Internal error in rule | All | Error |
| LX100 | RXT101 | Grid.Row without RowDefinition | All | Warning |
| LX101 | RXT102 | Grid.Column without ColumnDefinition | All | Warning |
| LX102 | RXT103 | Grid.RowSpan exceeds available rows | All | Warning |
| LX103 | RXT104 | Grid.ColumnSpan exceeds available columns | All | Warning |
| LX200 | RXT160 | SelectedItem binding should be TwoWay | All | Info |
| LX201 | RXT170 | Prefer x:Bind over Binding | Uwp, WinUI3 | Info |
| LX300 | RXT452 | x:Name should start with uppercase | All | Warning |
| LX301 | RXT451 | x:Uid should start with uppercase | Uwp, WinUI3 | Warning |
| LX400 | RXT200 | Hardcoded string; use resource | All | Info |
| LX500 | RXT150 | TextBox lacks InputScope | Uwp, WinUI3 | Info |
| LX501 | RXT330 | Slider Minimum > Maximum | Wpf, Maui | Warning |
| LX502 | RXT335 | Stepper Minimum > Maximum | Maui | Warning |
| LX600 | RXT402 | MediaElement deprecated — use MediaPlayerElement | Uwp, WinUI3 | Warning |

Rule files live at `src/XamlLint.Core/Rules/<Category>/LX###_DescriptiveName.cs` — subfolder per category, ID in filename for fast navigation. ID stability is enforced by `AnalyzerReleases.Shipped.md` (see §11).

## 4. Dialect detection cascade

For each linted file, resolve dialect in this order (first match wins):

1. **CLI `--dialect <name>`** flag.
2. **Nearest project config** (`xaml-lint.config.json`) — first matching `overrides[].files` glob, else `defaultDialect`.
3. **User-global config** `defaultDialect`.
4. **Xmlns sniff** — only for definitive dialects with unique root URLs:
   - MAUI: `http://schemas.microsoft.com/dotnet/2021/maui`
   - Avalonia: `https://github.com/avaloniaui`
   - Uno: WPF/UWP URL + Uno-specific ignorable markers
5. **Fallback**: `Wpf`. Logged at `--verbose`.

The csproj-walking heuristic is **explicitly rejected** — views often live several folders away from the relevant csproj, TFMs aren't reliable dialect signals (e.g., `net8.0-windows10.0.19041.0` is used for both WPF and WinUI 3), and the walk is fragile. Configuration is the intended signal.

## 5. Configuration

### 5.1 File locations

- **Project config**: `xaml-lint.config.json` at repo root or any ancestor of a linted file. Discovered by walking up from the linted file until a match, a `.git` directory, or filesystem root. Nearest config wins.
- **User-global config**: `%APPDATA%/xaml-lint/config.json` (Windows), `~/.config/xaml-lint/config.json` (Unix, respects `XDG_CONFIG_HOME`). Fallback when no project config is found.
- **CLI overrides**: `--config <path>`, `--dialect <name>`, `--no-config`.

Configs do not merge across levels in v1 — first match end-to-end wins. (Merging deferred until demonstrated need.)

### 5.2 Schema

```json
{
  "$schema": "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json",
  "defaultDialect": "wpf",
  "overrides": [
    { "files": "src/winui/**/*.xaml", "dialect": "winui3" },
    { "files": "**/*.Designer.xaml", "rules": { "LX400": "off" } }
  ],
  "rules": {
    "LX300": "off",
    "LX400": "warning",
    "LX100": "error"
  }
}
```

**Fields:**
- `defaultDialect` (required in v1): `wpf` | `winui3` | `uwp` | `maui` | `avalonia` | `uno`.
- `overrides[]` (optional): each has `files` (glob), `dialect?`, `rules?`. First match per file wins.
- `rules` (optional): map rule ID → `"error" | "warning" | "info" | "off"`. `"*"` applies to all rules (global severity override).

**Severity resolution order** for `(file, rule)`:

1. Rule's declared `Dialects` doesn't include detected dialect → **skipped** (not reported, not counted).
2. Start with `rule.Metadata.DefaultSeverity`.
3. Apply `config.rules[ruleId]` if present.
4. Apply first matching `config.overrides[].rules[ruleId]` if present.
5. CLI flags (`--error-on`, `--warning-on`) applied last — flag grammar deferred to post-v1.

### 5.3 Schema discovery

JSON Schema file shipped at `schema/v1/config.json`, hosted at `https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json`. Config files reference it via `$schema` — VS Code, Rider, and most editors pick up autocomplete/validation automatically. Migration to GitHub Pages (cleaner URL) is planned before v1 tag.

### 5.4 Malformed config

- Unreadable, malformed JSON, unknown `defaultDialect`, malformed glob: emit `LX003`, exit `2`, lint nothing. Error message includes config file path and failing key.
- Unknown rule ID in `rules` map: emit warning, continue linting (forward-compat).

## 6. Output formats

### 6.1 Format selection

- Stdout is TTY → default `pretty`.
- Stdout is piped → default `compact-json`.
- `--format <name>` overrides: `compact-json` | `sarif` | `msbuild` | `pretty`.
- `--output <path>` writes to file (defaults to stdout).

TTY-aware default matches `eslint`, `clippy`, `dotnet format`.

### 6.2 compact-json

Stable envelope:

```json
{
  "version": "1",
  "tool": { "name": "xaml-lint", "version": "0.1.0" },
  "results": [
    {
      "file": "src/Views/MainView.xaml",
      "ruleId": "LX300",
      "severity": "warning",
      "message": "x:Name 'myButton' should start with uppercase.",
      "startLine": 12,
      "startCol": 28,
      "endLine": 12,
      "endCol": 38,
      "helpUri": "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX300.md"
    }
  ]
}
```

Envelope is always emitted, even for clean files. The hook script skips forwarding empty results to Claude so context stays clean.

### 6.3 sarif

SARIF 2.1.0. Key mappings:

- `runs[0].tool.driver` — name, version, informationUri, `rules[]` metadata (id, name, shortDescription, helpUri, defaultConfiguration.level).
- `runs[0].results[]` — one entry per diagnostic with `ruleId`, `level` (SARIF `error`/`warning`/`note`), `message.text`, `locations[0].physicalLocation.artifactLocation.uri` (repo-relative) and `.region` (startLine/startColumn/endLine/endColumn).
- `suppressions[]` — diagnostics that fired but were pragma-suppressed, tagged `kind: "inSource"`. Gives CI visibility into suppression review.

### 6.4 msbuild

One line per diagnostic:

```
src/Views/MainView.xaml(12,28): warning LX300: x:Name 'myButton' should start with uppercase. [https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX300.md]
```

Format: `FILE(LINE,COL): severity RULEID: message [helpUri]`. Multi-line ranges collapse to start position only. Silent on clean.

### 6.5 pretty

Colored (ANSI), honors `NO_COLOR`. Headers per file, aligned columns. Clean file: `No issues found.` (plain text, no emoji).

### 6.6 Exit codes (across all formats)

- `0` — no findings, or only `warning`/`info`.
- `1` — at least one `error` severity diagnostic.
- `2` — tool-level failure (unreadable input, malformed config, engine crash).

Precedence when multiple apply: `2 > 1 > 0`.

### 6.7 Flags in v1

- `--format <name>`
- `--output <path>`
- `--config <path>`
- `--no-config`
- `--dialect <name>`
- `--only LX100,LX101,...` — allow-list
- `--quiet` — suppress `info` severity from output (still affect nothing)
- `--force` — lint a file whose extension isn't `.xaml`
- `--verbose` — engine logs to stderr

Deferred to post-v1: `--error-on warning`, `--fix`, `--watch`.

## 7. Error handling

### 7.1 Per-file

- **File not readable** — emit `LX004`, continue. Exit code accrues to `2`.
- **Malformed XAML** — emit `LX001` with parser's line/col, skip rule evaluation for that file.
- **Unknown encoding** — assume UTF-8; honor BOM (UTF-8/UTF-16-LE/UTF-16-BE); honor XML declaration's encoding via `XmlReader`. If still undecodable, treat as malformed XAML (`LX001`).
- **Empty file** — zero diagnostics.
- **Non-XAML extension** — emit `LX005` at `info`, skip. `--force` overrides.

### 7.2 Config

- Malformed JSON, unknown dialect, malformed glob, schema violations: `LX003`, exit `2`, lint nothing.
- Unknown rule ID: warning, continue.

### 7.3 Rule crashes

Each rule invocation is `try/catch`-wrapped. Unhandled exception → emit `LX006` with exception type and short context, skip that rule for that file, continue. Full stack trace to stderr (or `--verbose` stdout). Diagnostics output stays clean for consumers. `LX006` is `error`-severity; a crash shouldn't silently pass CI.

### 7.4 Stderr vs stdout

Strictly separated. Stdout is formatted output. Stderr is log messages, stack traces under `--verbose`, progress info. Claude's hook reads stdout; stderr is for humans debugging.

## 8. Testing

### 8.1 Stack

- **xUnit v3** with **Microsoft Testing Platform** via `xunit.v3.mtp-v2` NuGet package.
- Test projects are self-hosting executables (`<EnableMicrosoftTestingPlatform>true</EnableMicrosoftTestingPlatform>` in csproj).
- **AwesomeAssertions** (MIT fork of FluentAssertions) for assertions.
- Snapshot comparisons via a small custom JSON normalizer (no `Verify.Xunit` dep unless it demonstrably pays off).

### 8.2 Test layers

1. **Rule unit tests** — directory-per-rule, fixture-driven:
   ```
   tests/XamlLint.Core.Tests/Rules/LX100/
     invalid-missing-row/
       input.xaml
       expected.json
     valid-with-definitions/
       input.xaml
       expected.json      # []
     ...
   ```
   Test body loads inputs, runs the single rule, asserts canonical JSON match. ~10 fixtures per rule is typical.

2. **Engine integration tests** — full `XamlDocument → SuppressionMap → rule dispatch → Diagnostics[]` pipeline. Covers pragma handling, dialect gating, severity resolution, multi-rule interaction.

3. **CLI integration tests** — spawn the CLI binary, feed args, assert stdout/stderr/exit code. Covers config discovery, format selection, TTY-aware defaults, `--only`, `--output`, glob expansion.

4. **Plugin end-to-end smoke test** — invoke the hook subcommand with canned `tool_input.file_path` JSON on stdin, assert stdout is valid compact-json. Guards against hook rot.

5. **Meta-tests** — reflect over the source-generated `GeneratedRuleCatalog.Rules` list and assert catalog invariants:
   - No duplicate rule IDs.
   - Every rule ID matches a row in `AnalyzerReleases.Shipped.md` or `AnalyzerReleases.Unshipped.md` (exactly one).
   - Every rule has a `docs/rules/LX###.md` file.
   - Every rule's `HelpUri` matches the expected URL pattern (`https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX###.md` at v1; migrates to GitHub Pages before v1 tag).
   - `Dialects` is non-zero for every rule.
   - `UpstreamId`, when present, follows the `RXT\d+` pattern.
   - Rule class filename matches rule ID (`LX100_GridRowWithoutRowDefinition.cs`).

   Meta-tests catch whole classes of "forgot to update one of five files" bugs cheaply — they run on every CI build.

6. **Performance budget** — benchmark test (BenchmarkDotNet) asserting a representative 200-line WPF view lints in under **50ms p95**. CI fails on regression.

### 8.3 CI

GitHub Actions matrix: `{windows-latest, ubuntu-latest, macos-latest}` × `{net8, net9}`. Release workflow on tag push: `dotnet pack` + `dotnet nuget push`. Dry-run of publish on release-candidate branches.

### 8.4 Dogfooding

Test-fixture XAML files themselves lint in a CI step. Forces fixtures to be deliberately valid or deliberately malformed (annotated). Self-linting the plugin repo's own XAML is a stretch goal post-v1 (requires real XAML in the repo first).

## 9. Project layout

```
xaml-lint/
├── .claude-plugin/
│   └── plugin.json
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── release.yml
├── .gitignore
├── AnalyzerReleases.Shipped.md      # append-only rule ID/severity log (v1, v2, ...)
├── AnalyzerReleases.Unshipped.md    # entries for the next release; graduate on tag
├── CHANGELOG.md                     # human-readable release notes
├── CLAUDE.md
├── Directory.Build.props            # shared MSBuild properties across projects
├── Directory.Packages.props         # central package version management
├── LICENSE
├── README.md
├── version.json                     # Nerdbank.GitVersioning config
├── xaml-lint.slnx                   # SLNX format (not legacy SLN)
├── commands/
│   └── lint.md                      # /xaml-lint:lint <path>
├── skills/
│   └── lint-xaml/
│       └── SKILL.md
├── hooks/
│   └── hooks.json                   # PostToolUse → xaml-lint hook
├── docs/
│   ├── superpowers/
│   │   └── specs/
│   │       └── 2026-04-17-xaml-lint-design.md
│   └── rules/
│       ├── tool.md                  # category overview (LX0xx)
│       ├── layout.md                # category overview (LX1xx)
│       ├── bindings.md              # category overview (LX2xx)
│       ├── naming.md                # category overview (LX3xx)
│       ├── resources.md             # category overview (LX4xx)
│       ├── input.md                 # category overview (LX5xx)
│       ├── deprecated.md            # category overview (LX6xx)
│       └── LX001.md … LX600.md      # per-rule docs (non-contiguous IDs)
├── schema/
│   └── v1/
│       └── config.json
├── src/
│   ├── XamlLint.Core/               # rule engine (net8.0 class library)
│   │   ├── XamlLint.Core.csproj     # EmitCompilerGeneratedFiles=true
│   │   ├── XamlDocument.cs
│   │   ├── Diagnostic.cs
│   │   ├── Dialect.cs
│   │   ├── IXamlRule.cs
│   │   ├── RuleMetadata.cs
│   │   ├── RuleContext.cs
│   │   ├── Helpers/                 # one helper class per domain concept
│   │   │   ├── XamlNameHelpers.cs   # x:Name, TargetName, attached-prop qualifiers
│   │   │   ├── MarkupExtensionHelpers.cs
│   │   │   ├── NamespaceHelpers.cs
│   │   │   └── LocationHelpers.cs
│   │   ├── Suppressions/
│   │   │   ├── PragmaParser.cs
│   │   │   └── SuppressionMap.cs
│   │   ├── Parsing/
│   │   │   ├── XamlParser.cs
│   │   │   └── DialectDetector.cs
│   │   └── Rules/
│   │       ├── Tool/                # LX001–LX099
│   │       │   ├── LX001_MalformedXaml.cs
│   │       │   └── LX002_UnrecognizedPragma.cs
│   │       ├── Layout/              # LX100–LX199
│   │       │   └── LX100_GridRowWithoutRowDefinition.cs …
│   │       ├── Bindings/            # LX200–LX299
│   │       ├── Naming/              # LX300–LX399
│   │       ├── Resources/           # LX400–LX499
│   │       ├── Input/               # LX500–LX599
│   │       └── Deprecated/          # LX600–LX699
│   ├── XamlLint.Core.SourceGen/     # netstandard2.0 analyzer/generator project
│   │   ├── RuleCatalogGenerator.cs  # emits GeneratedRuleCatalog.Rules + Metadata
│   │   └── RuleDocStubGenerator.cs  # emits docs/rules/LX###.md stubs if missing
│   └── XamlLint.Cli/                # dotnet tool (net8.0, PackAsTool=true)
│       ├── XamlLint.Cli.csproj
│       ├── Program.cs
│       ├── Commands/
│       │   ├── LintCommand.cs
│       │   └── HookCommand.cs
│       ├── Config/
│       │   ├── ConfigLoader.cs
│       │   └── ConfigSchema.cs
│       └── Formatters/
│           ├── CompactJsonFormatter.cs
│           ├── SarifFormatter.cs
│           ├── MsBuildFormatter.cs
│           └── PrettyFormatter.cs
└── tests/
    ├── XamlLint.Core.Tests/          # rule unit tests (Rules/<Category>/LX###/fixture/...)
    │   ├── Meta/                     # catalog invariant tests (see §8.2)
    │   └── Rules/
    │       ├── Tool/
    │       ├── Layout/
    │       └── …
    ├── XamlLint.Cli.Tests/           # CLI integration
    └── XamlLint.Plugin.Tests/        # hook shim smoke test
```

Three csproj projects: `XamlLint.Core` (class library), `XamlLint.Core.SourceGen` (analyzers/generators project, `netstandard2.0`), `XamlLint.Cli` (exe, `<PackAsTool>true</PackAsTool>`, `<ToolCommandName>xaml-lint</ToolCommandName>`).

**Shared MSBuild**: `Directory.Build.props` pins common settings (nullable, treat-warnings-as-errors, target framework). `Directory.Packages.props` uses central package management so NuGet versions are managed in one place.

**Versioning**: `Nerdbank.GitVersioning` (preferred) or `MinVer` drives SemVer from git tags. `version.json` at repo root. No hand-edited version strings in csproj files.

## 10. Milestones

Each milestone is an independently dogfood-able plugin.

**Release discipline across all milestones:** every new rule or severity change lands first in `AnalyzerReleases.Unshipped.md`. Graduating a release copies those entries to `AnalyzerReleases.Shipped.md` under the new version header. Meta-tests (§8.2) enforce that every catalog ID has exactly one row across the two files. No ID is ever deleted — removals move to a `### Removed Rules` subsection.

### M0 — Scaffold (pre-release)
- `.slnx` + three csproj projects building clean
- `Directory.Build.props`, `Directory.Packages.props`, `version.json` (Nerdbank.GitVersioning)
- Empty `AnalyzerReleases.Shipped.md` + `AnalyzerReleases.Unshipped.md`
- CI pipeline green on all three OSes (win/linux/mac)
- Not a user-visible release

### M1 — Plumbing end-to-end (v0.1.0)
- Tool/engine diagnostics: `LX001`–`LX006` (all six)
- No lint rules yet — M1 validates the engine, CLI, config, plugin, and test harness end-to-end
- Engine: `IXamlRule`, `XamlDocument`, source-gen rule catalog, source-gen doc-stub generator, pragma parsing, `SuppressionMap`
- Meta-tests: catalog invariants green
- CLI: `lint` + `hook` subcommands, all v1 flags (§6.7)
- Config: discovery cascade, project + user-global, schema file
- Plugin veneer: manifest, hook, skill, slash command
- Docs: README install + quickstart, rule docs for LX001–LX006, category-overview page for `tool.md`, config reference
- `AnalyzerReleases.Unshipped.md` → `Shipped.md` graduation on tag
- Published to NuGet as `dotnet tool`

### M2 — Easy rules (v0.2.0)
- `LX200` SelectedItem TwoWay, `LX300` x:Name casing, `LX400` hardcoded strings
- Rule docs + category-overview pages: `bindings.md`, `naming.md`, `resources.md`
- `AnalyzerReleases.Unshipped.md` → `Shipped.md` graduation on tag

### M3 — Grid family (v0.3.0)
- `LX100`, `LX101`, `LX102`, `LX103`
- Exercises Grid-ancestry traversal, attached-property + element-syntax variants
- Category-overview page: `layout.md`
- `AnalyzerReleases.Unshipped.md` → `Shipped.md` graduation on tag

### M4 — Dialect-gated rules (v0.4.0)
- `LX201` prefer x:Bind, `LX301` x:Uid casing, `LX500` TextBox InputScope, `LX501` Slider min/max, `LX502` Stepper min/max, `LX600` MediaElement deprecated
- Exercises dialect-gated rule execution across UWP/WinUI 3/MAUI
- Category-overview pages: `input.md`, `deprecated.md`
- `AnalyzerReleases.Unshipped.md` → `Shipped.md` graduation on tag

### v1.0.0 release
- M4 complete; all rule IDs live (13 lint rules + 6 tool/engine diagnostics)
- Rule docs migrated to GitHub Pages (cleaner URLs); `HelpUri` scheme updated across rules + meta-test
- Potential repo move to a GitHub organization (before or immediately after tag)
- Announcement, plugin marketplace submission
- `CHANGELOG.md` written, cross-referencing `AnalyzerReleases.Shipped.md`
- Versioning policy: semver; rule additions minor, rule removals major, severity downgrades minor, severity upgrades major

## 11. Attribution and rule documentation

### 11.1 Attribution policy

- `LICENSE` preserves Matt Lacey's copyright line alongside the current author's.
- `README.md` credits Rapid XAML Toolkit in the Attribution section.
- Each ported rule file carries a one-line header comment: `// Ported from Rapid XAML Toolkit's RXT101 (c) Matt Lacey Ltd., MIT.`
- `RuleMetadata.UpstreamId` carries the upstream rule ID programmatically for cross-reference.

### 11.2 Rule doc template

Every per-rule doc file follows the 4-heading template (modeled on StyleCopAnalyzers):

```markdown
# LX100: Grid.Row without matching RowDefinition

<!-- generated stub; edit freely. Upstream: RXT101. -->

## Cause

A one-sentence description of what triggers the rule.

## Rule description

Longer-form explanation. Why the pattern is a problem, what the correct form
looks like, and one or two code snippets showing both.

## How to fix violations

Concrete steps the reader takes. For example: "Add a `<RowDefinition>` for each
distinct `Grid.Row` value used by the grid's children, or remove the `Grid.Row`
attribute to let the child occupy the default row."

## How to suppress violations

Copy-pasteable snippets for every suppression mechanism we support:

- `<!-- xaml-lint disable once LX100 -->` (inline, one element)
- `<!-- xaml-lint disable LX100 -->` … `<!-- xaml-lint restore LX100 -->` (block)
- `xaml-lint.config.json` → `"rules": { "LX100": "off" }` (file/project)
```

Every rule's doc file is stubbed on first build by the `RuleDocStubGenerator` if missing. The stub contains all four headings with placeholder text; authors replace the placeholders. Meta-tests assert every rule has a non-stub file (no placeholder text left in shipped docs — simple grep on a sentinel).

### 11.3 Category overview pages

One file per category at `docs/rules/<category>.md` (e.g., `layout.md`, `naming.md`). Each page has a short intro and a table linking to every rule in that range. Generated by `RuleDocStubGenerator` on first build; authors maintain the intro.

## 12. Deferred / post-v1

- **LSP server** — strong candidate for v2. Claude Code has first-class plugin LSP support; running a persistent server beats per-edit CLI invocations for warm caches, push diagnostics, and cross-file awareness. Non-Claude editor users (VS Code, Rider, Neovim) also benefit. v1's stateless engine is explicitly designed so the v2 LSP wrap is additive.
- **Auto-fix** — `xaml-lint --fix`. Rules declare `Fix()` methods; CLI applies in-place. Preserving XAML formatting under fixes is nontrivial; deferred until real demand is validated.
- **`--error-on`/`--warning-on` severity promotion** — useful for strict CI. Grammar needs design; defer.
- **Config merging across levels** — if v1 users hit friction with "first match wins" semantics.
- **Automatic `dotnet tool install`** on plugin enable — revisit once Anthropic's platform support is understood.
- **Self-hosting the plugin on xaml-lint repo** — requires real XAML fixtures to exist first.
- **Corpus regression tester** — a small console app that runs the CLI over a curated list of open-source XAML repos and diffs SARIF output against a committed baseline. Catches behavioral drift early (StyleCopAnalyzers has an equivalent: `StyleCopTester`). Post-v1 quality tool; don't block v1 on it.
- **Rule capability matrix page** — auto-generated markdown table reflecting over the catalog (which rules have codefixes, which are enabled-by-default, which are dialect-specific). Cheap once `[NoCodeFix]`-style marker attributes exist.

## 13. Open items before v1 tag

- Decide and migrate to GitHub Pages for schema + rule docs URLs (currently raw blob URLs).
- Decide repo location (personal `jizc/` vs new organization). Migration before v1 tag is preferred to avoid link breakage.
- Write CHANGELOG and versioning policy in `CONTRIBUTING.md` / `CHANGELOG.md`.
- NuGet package readme + icon.
- Plugin marketplace submission materials.
