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
    Id = "LX010",
    UpstreamId = "RXT101",
    Title = "Grid.Row used without matching RowDefinition",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX010.md")]
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

Runtime has zero reflection and zero MEF. AOT-friendly.

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

`LX001`–`LX009` reserved for tool/engine diagnostics. Rules start at `LX010`.

| ID | Upstream | Title | Dialects | Default |
|---|---|---|---|---|
| LX001 | RXT999 | Malformed XAML | All | Error |
| LX002 | — | Unrecognized pragma directive | All | Warning |
| LX003 | — | Malformed configuration | All | Error |
| LX004 | — | Cannot read file | All | Error |
| LX005 | — | Skipping non-XAML file | All | Info |
| LX006 | — | Internal error in rule | All | Error |
| LX010 | RXT101 | Grid.Row without RowDefinition | All | Warning |
| LX011 | RXT102 | Grid.Column without ColumnDefinition | All | Warning |
| LX012 | RXT103 | Grid.RowSpan exceeds available rows | All | Warning |
| LX013 | RXT104 | Grid.ColumnSpan exceeds available columns | All | Warning |
| LX014 | RXT160 | SelectedItem binding should be TwoWay | All | Info |
| LX015 | RXT200 | Hardcoded string; use resource | All | Info |
| LX016 | RXT452 | x:Name should start with uppercase | All | Warning |
| LX017 | RXT150 | TextBox lacks InputScope | Uwp, WinUI3 | Info |
| LX018 | RXT170 | Prefer x:Bind over Binding | Uwp, WinUI3 | Info |
| LX019 | RXT330 | Slider Minimum > Maximum | Wpf, Maui | Warning |
| LX020 | RXT335 | Stepper Minimum > Maximum | Maui | Warning |
| LX021 | RXT402 | MediaElement deprecated — use MediaPlayerElement | Uwp, WinUI3 | Warning |
| LX022 | RXT451 | x:Uid should start with uppercase | Uwp, WinUI3 | Warning |

Rule files live at `src/XamlLint.Core/Rules/LX###_DescriptiveName.cs` (ID in filename for fast navigation).

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
    { "files": "**/*.Designer.xaml", "rules": { "LX015": "off" } }
  ],
  "rules": {
    "LX016": "off",
    "LX015": "warning",
    "LX010": "error"
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
      "ruleId": "LX016",
      "severity": "warning",
      "message": "x:Name 'myButton' should start with uppercase.",
      "startLine": 12,
      "startCol": 28,
      "endLine": 12,
      "endCol": 38,
      "helpUri": "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX016.md"
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
src/Views/MainView.xaml(12,28): warning LX016: x:Name 'myButton' should start with uppercase. [https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX016.md]
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
- `--only LX010,LX011,...` — allow-list
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
   tests/XamlLint.Core.Tests/Rules/LX010/
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

5. **Performance budget** — benchmark test (BenchmarkDotNet) asserting a representative 200-line WPF view lints in under **50ms p95**. CI fails on regression.

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
├── CLAUDE.md
├── LICENSE
├── README.md
├── xaml-lint.slnx                  # SLNX format (not legacy SLN)
├── commands/
│   └── lint.md                     # /xaml-lint:lint <path>
├── skills/
│   └── lint-xaml/
│       └── SKILL.md
├── hooks/
│   └── hooks.json                  # PostToolUse → xaml-lint hook
├── docs/
│   ├── superpowers/
│   │   └── specs/
│   │       └── 2026-04-17-xaml-lint-design.md
│   └── rules/
│       └── LX001.md … LX022.md
├── schema/
│   └── v1/
│       └── config.json
├── src/
│   ├── XamlLint.Core/              # rule engine
│   │   ├── XamlLint.Core.csproj
│   │   ├── XamlDocument.cs
│   │   ├── Diagnostic.cs
│   │   ├── Dialect.cs
│   │   ├── IXamlRule.cs
│   │   ├── RuleMetadata.cs
│   │   ├── RuleContext.cs
│   │   ├── Suppressions/
│   │   │   ├── PragmaParser.cs
│   │   │   └── SuppressionMap.cs
│   │   ├── Parsing/
│   │   │   ├── XamlParser.cs
│   │   │   └── DialectDetector.cs
│   │   └── Rules/
│   │       ├── LX001_MalformedXaml.cs
│   │       └── LX010_GridRowWithoutRowDefinition.cs …
│   ├── XamlLint.Core.SourceGen/    # build-time rule catalog generator
│   │   └── RuleCatalogGenerator.cs
│   └── XamlLint.Cli/               # dotnet tool
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
    ├── XamlLint.Core.Tests/
    ├── XamlLint.Cli.Tests/
    └── XamlLint.Plugin.Tests/
```

Three csproj projects: `XamlLint.Core` (class library), `XamlLint.Core.SourceGen` (analyzers/generators project, `netstandard2.0`), `XamlLint.Cli` (exe, `<PackAsTool>true</PackAsTool>`, `<ToolCommandName>xaml-lint</ToolCommandName>`).

## 10. Milestones

Each milestone is an independently dogfood-able plugin.

### M0 — Scaffold (pre-release)
- `.slnx` + three csproj projects building clean
- CI pipeline green on all three OSes
- Not a user-visible release

### M1 — Plumbing end-to-end (v0.1.0)
- Tool/engine diagnostics: `LX001`–`LX006` (all six)
- No lint rules yet — M1 validates the engine, CLI, config, plugin, and test harness end-to-end
- Engine: `IXamlRule`, `XamlDocument`, source-gen rule catalog, pragma parsing, `SuppressionMap`
- CLI: `lint` + `hook` subcommands, all v1 flags (§6.7)
- Config: discovery cascade, project + user-global, schema file
- Plugin veneer: manifest, hook, skill, slash command
- Docs: README install + quickstart, rule docs for LX001/LX002, config reference
- Published to NuGet as `dotnet tool`

### M2 — Easy rules (v0.2.0)
- `LX014` SelectedItem TwoWay, `LX015` hardcoded strings, `LX016` x:Name casing
- Rule docs for each

### M3 — Grid family (v0.3.0)
- `LX010`, `LX011`, `LX012`, `LX013`
- Exercises Grid-ancestry traversal, attached-property + element-syntax variants

### M4 — Dialect-gated rules (v0.4.0)
- `LX017` TextBox InputScope, `LX018` prefer x:Bind, `LX019` Slider min/max, `LX020` Stepper min/max, `LX021` MediaElement deprecated, `LX022` x:Uid casing
- Exercises dialect-gated rule execution across UWP/WinUI 3/MAUI

### v1.0.0 release
- M4 complete; all rule IDs live (13 lint rules + 6 tool/engine diagnostics)
- Rule docs migrated to GitHub Pages (cleaner URLs)
- Potential repo move to a GitHub organization (before or immediately after tag)
- Announcement, plugin marketplace submission
- CHANGELOG, versioning policy: semver, rule additions are minor, rule removals are major

## 11. Attribution policy

- `LICENSE` preserves Matt Lacey's copyright line alongside the current author's.
- `README.md` credits Rapid XAML Toolkit in the Attribution section.
- Each ported rule file carries a one-line header comment: `// Ported from Rapid XAML Toolkit's RXT101 (c) Matt Lacey Ltd., MIT.`
- `RuleMetadata.UpstreamId` carries the upstream rule ID programmatically for cross-reference.

## 12. Deferred / post-v1

- **LSP server** — strong candidate for v2. Claude Code has first-class plugin LSP support; running a persistent server beats per-edit CLI invocations for warm caches, push diagnostics, and cross-file awareness. Non-Claude editor users (VS Code, Rider, Neovim) also benefit. v1's stateless engine is explicitly designed so the v2 LSP wrap is additive.
- **Auto-fix** — `xaml-lint --fix`. Rules declare `Fix()` methods; CLI applies in-place. Preserving XAML formatting under fixes is nontrivial; deferred until real demand is validated.
- **`--error-on`/`--warning-on` severity promotion** — useful for strict CI. Grammar needs design; defer.
- **Config merging across levels** — if v1 users hit friction with "first match wins" semantics.
- **Automatic `dotnet tool install`** on plugin enable — revisit once Anthropic's platform support is understood.
- **Self-hosting the plugin on xaml-lint repo** — requires real XAML fixtures to exist first.

## 13. Open items before v1 tag

- Decide and migrate to GitHub Pages for schema + rule docs URLs (currently raw blob URLs).
- Decide repo location (personal `jizc/` vs new organization). Migration before v1 tag is preferred to avoid link breakage.
- Write CHANGELOG and versioning policy in `CONTRIBUTING.md` / `CHANGELOG.md`.
- NuGet package readme + icon.
- Plugin marketplace submission materials.
