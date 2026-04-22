# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Rule-level history is tracked in [AnalyzerReleases.Shipped.md](AnalyzerReleases.Shipped.md).

## [Unreleased]

### Added

- CLI `--rule <ID>:<severity>` flag for ad-hoc rule-severity overrides. Severity values: `off` / `info` / `warning` / `error`. Repeatable and CSV-stackable on the short form (`--rule LX100:warning,LX200:off`); object form `--rule '{"ID":"severity",...}'` mirrors the `xaml-lint.config.json` `rules:` schema for forward-compatibility with future rule options. Applied on top of config-resolved severities, below inline pragmas.
- CLI `--preset <recommended|strict|none>` flag. Overrides any `extends:` in the config file. `none` disables every rule; combine with `--rule` to build an isolated rule set for one invocation.
- CLI `--no-inline-config` flag. Ignores `<!-- xaml-lint disable ... -->` pragmas inside source files — useful for CI audit passes that want to see the unvarnished diagnostic set.
- CLI `-c` short alias for `--config`.
- [LX702](docs/rules/LX702.md) — TextBox lacks accessibility description. Covers WPF, WinUI 3, UWP, Avalonia, and Uno (MAUI is covered by LX703). Port of upstream RXT601. Off by default in `:recommended`.
- [LX703](docs/rules/LX703.md) — Entry lacks accessibility description. MAUI-original sibling to LX702. Off by default in `:recommended`.
- [LX800](docs/rules/LX800.md) — Uno platform XML namespace must be `mc:Ignorable`. Port of upstream RXT700; opens the Platform category (LX800–LX899). On in `:recommended` at `warning`.
- Scope-aware `XamlNameIndex` infrastructure backing `{x:Reference}` validation. Templates (`ControlTemplate`/`DataTemplate`/`ItemsPanelTemplate`/`HierarchicalDataTemplate`) isolate names from the outer scope — cross-template references are rejected, matching XAML runtime semantics.

### Changed

- CLI `--only` is now a shorthand for `--preset none --no-config-lookup --rule ID:<severity>...` rather than a pure output filter. Bare IDs use the rule's `DefaultSeverity`; explicit severities (`--only LX700:warning`) override. Mutually exclusive with `--preset` / `--rule` / `--config` / `--no-config-lookup`. The old filter-only `--only` is gone — prior usage that relied on it respecting `xaml-lint.config.json`'s on/off switch will see different behaviour.
- CLI `--no-config` renamed to `--no-config-lookup` (matches eslint). **Breaking.**
- Dialect coverage corrections, verified against official documentation (`unoplatform/uno` docs and `AvaloniaUI/Avalonia` source):
  - [LX201](docs/rules/LX201.md) (Prefer x:Bind over Binding) now applies to Uno Platform in addition to UWP and WinUI 3 — Uno supports `{x:Bind}` via its WinUI-compatible compiler.
  - [LX301](docs/rules/LX301.md) (x:Uid casing) now applies to Uno Platform — Uno uses UWP-style `x:Uid` + `.resw` localization.
  - [LX500](docs/rules/LX500.md) (TextBox lacks InputScope) now applies to Uno Platform — `Windows.Input.InputScopeNameValue` is fully implemented across all Uno platform heads.
  - [LX501](docs/rules/LX501.md) (Slider Min > Max) now applies to Avalonia — Avalonia's `RangeBase.CoerceMaximum` silently clamps Max up to Min via `Math.Max`, so an inconsistent pair authored in XAML quietly loses the stated range at runtime.
  - [LX600](docs/rules/LX600.md) (MediaElement deprecated) now applies to Uno Platform — Uno inherits the UWP/WinUI deprecation and also ships `MediaPlayerElement` as the preferred control.
  - [LX601](docs/rules/LX601.md) (Line.Fill has no effect) now applies to all dialects — a line's geometry has zero interior area, so `Fill` is a universal no-op across WPF, WinUI 3, UWP, MAUI, Avalonia, and Uno. Previously MAUI-only, which missed the same bug on every other dialect.
  - [LX700](docs/rules/LX700.md) (Image lacks accessibility description) now applies to all dialects — `AutomationProperties.Name`/`HelpText`/`LabeledBy` exist across WPF, WinUI 3, UWP, MAUI, Avalonia, and Uno.
- [LX700](docs/rules/LX700.md) and [LX701](docs/rules/LX701.md) — `AutomationProperties.LabeledBy="{x:Reference <name>}"` now suppresses the rule only when the referenced name is declared in the same XAML name scope as the image. Dangling references (typo'd targets, deleted elements, cross-template references) now fire. Behaviour is unchanged for non-reference literals and for `{Binding …}` / other markup extensions.
- [LX700](docs/rules/LX700.md), [LX701](docs/rules/LX701.md), and [LX702](docs/rules/LX702.md) — `AutomationProperties.LabeledBy="{Binding ElementName=<name>}"` is now recognised as an element reference on the same footing as `{x:Reference}` and scope-validated the same way. The `{Binding ElementName=…}` form is the dominant WPF element-reference idiom (predates `x:Reference`) and was previously swept under the permissive "any other markup extension suppresses" branch — dangling `ElementName` targets wrongly suppressed. `{Binding Path=…}` without `ElementName` still suppresses permissively since it can't be statically resolved to an element.
- [LX702](docs/rules/LX702.md) — reverse-direction WPF labeling via `<Label Target="{x:Reference <name>}">` or `<Label Target="{Binding ElementName=<name>}">` now suppresses the diagnostic on the referenced `TextBox`. Matches the XAML runtime semantics where `Label.Target` wires the automation peer so screen readers announce the Label's content as the input's name. Scope isolation applies: a `Label` inside a `ControlTemplate` / `DataTemplate` only suppresses TextBoxes declared in the same template scope.

### Fixed

- [LX700](docs/rules/LX700.md) and [LX701](docs/rules/LX701.md) — MAUI's `SemanticProperties.Description` and `SemanticProperties.Hint` now suppress the rule. The idiomatic MAUI accessibility markup was previously flagged as missing even though `SemanticProperties.*` is the canonical way to attach an AT name/hint on MAUI. Matches LX703's existing behaviour.
- [LX301](docs/rules/LX301.md) — no longer false-positives on UWP/WinUI `x:Uid` values in the `/ResourceFile/Key` resw namespace-scope form. The casing convention now applies to the resource key (the segment after the final `/`) instead of the leading `/` character; `x:Uid="/resources/Description"` is compliant, `x:Uid="/resources/description"` still fires.
- Avalonia `.axaml` files are now linted as first-class XAML. Previously every `.axaml` path emitted [LX005](docs/rules/LX005.md) ("Skipping non-XAML file") and the rule pipeline never ran against it — even under `--force`. Both the CLI (positional paths, directory recursion) and the Claude Code hook now accept `.axaml` alongside `.xaml`.
- [LX400](docs/rules/LX400.md) — values whose non-whitespace characters contain no letters and no digits are now treated as non-localisable chrome and skipped. Covers both icon-font glyphs in the Unicode Private Use Area (Segoe MDL2 Assets, Segoe Fluent Icons, Material Icons, FontAwesome, and similar) and UI-chrome punctuation like `"+"`, `"-"`, `":"`, `"&lt;&lt;"`. Single letters (`"X"`), digits (`"1"` localises to `"١"` in Arabic), and mixed values (`"+ Add"`) still fire.
- `xaml-lint hook` no longer emits an `LX005` diagnostic for every non-XAML file Claude edits. The hook now short-circuits on any `tool_input.file_path` that doesn't end in `.xaml` (case-insensitive) before config discovery or the rule catalog load, writing an empty envelope to stdout. The `lint` subcommand's LX005 behavior is unchanged.
- Hook empty-envelope response now reports the actual tool version instead of a hardcoded `"dev"` literal. Both empty-payload and non-XAML code paths go through `CompactJsonFormatter`, so the shape matches every other hook response.

## [1.0.0] - 2026-04-19

Stable release. No behavior changes since v0.5.0 beyond the `main`-branch polish below. Starting here, `version.json` carries the full 3-segment version verbatim (no prerelease suffix, no git-height-as-patch) — published package version matches the git tag exactly.

### Added

- `DefaultEnabled` property on `XamlRuleAttribute` (defaults `true`). Rules marked `DefaultEnabled = false` are written as `"off"` in the `xaml-lint:recommended` preset — useful for signals that are valuable but too noisy for most projects out-of-the-box. `:strict` still enables them at the escalated severity, and users extending `:recommended` can opt in explicitly.

### Changed

- Multi-target `net8.0;net9.0;net10.0` for the shipped CLI tool and `XamlLint.Core` library. Previously `net10.0`-only, which required users to have the .NET 10 runtime installed to run the tool; now matches prevailing dotnet-tool practice (csharpier, dotnet-ef, reportgenerator, NSwag, etc. all multi-target the same set). The source generator stays `netstandard2.0`; the internal `DocTool` stays `net10.0` (it only runs in CI via `dotnet run`, which picks the highest-compatible TFM anyway).
- `xaml-lint:recommended` preset tuned based on dogfooding against a real ~1k-file WPF codebase — two rules are now off-by-default (`DefaultEnabled = false`) because they dominated output noise without catching real bugs:
  - [LX400](docs/rules/LX400.md) (hardcoded string → resource): `info` → `off`. Localization is opt-in; most apps aren't fully localized.
  - [LX300](docs/rules/LX300.md) (x:Name should start with uppercase): `warning` → `off`. Lowercase `x:Name` is common for template-internal or pure-layout names (`border`, `grid`, `PART_ContentHost`); style consistency is a team preference.
  - Both rules retain their original `DefaultSeverity` and still fire in `:strict` (LX400 `warning`, LX300 `error`); users who want them on in `:recommended` can enable them explicitly.

## [0.5.0] - 2026-04-19

M5 — pre-v1 polish. Rules-inert release focused on org migration, repo hygiene, release-surface polish, and contributor docs.

### Added

- Project branding: logo (`assets/logo.png`, `assets/logo.svg`), wired as NuGet `<PackageIcon>` — README and icon now ship at the nupkg root ([#6])

### Changed

- Repo transferred from `jizc/xaml-lint` to [`XAML-Lint/xaml-lint`](https://github.com/XAML-Lint/xaml-lint) ([#6])
- `HelpUri` owner slug flipped across 26 in-source sites (20 `[XamlRule]` attributes + 3 consts in `RuleDispatcher` / `PragmaParser` / `ConfigLoader` + 3 inline strings in `LintPipeline`). URL shape preserved: `https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX###.md` ([#6])
- `$id` / `$schema` URLs in `schema/v1/config.json`, the three bundled presets, `README.md`, and `docs/config-reference.md` now point at the new owner. URL shape preserved on `raw.githubusercontent.com` ([#6])
- Repo-metadata owner refs updated: `.claude-plugin/plugin.json` homepage, `Directory.Build.props` (`PackageProjectUrl` + `RepositoryUrl`), `CHANGELOG.md` compare/PR links, SARIF output `informationUri`, `docs/rules/LX006.md` issue-tracker link ([#6])

## [0.4.0] - 2026-04-18

M4 — dialect-gated rules spanning UWP/WinUI 3, .NET MAUI, and WPF.

### Added

- [LX201](docs/rules/LX201.md) — Prefer x:Bind over Binding ([#5])
- [LX301](docs/rules/LX301.md) — x:Uid should start with uppercase ([#5])
- [LX500](docs/rules/LX500.md) — TextBox lacks InputScope ([#5])
- [LX501](docs/rules/LX501.md) — Slider Minimum is greater than Maximum ([#5])
- [LX502](docs/rules/LX502.md) — Stepper Minimum is greater than Maximum ([#5])
- [LX600](docs/rules/LX600.md) — MediaElement is deprecated — use MediaPlayerElement ([#5])
- Category overview pages: [input](docs/rules/input.md), [deprecated](docs/rules/deprecated.md) ([#5])

## [0.3.0] - 2026-04-18

M3 — Grid-family layout rules.

### Added

- [LX100](docs/rules/LX100.md) — Grid.Row without matching RowDefinition ([#4])
- [LX101](docs/rules/LX101.md) — Grid.Column without matching ColumnDefinition ([#4])
- [LX102](docs/rules/LX102.md) — Grid.RowSpan exceeds available rows ([#4])
- [LX103](docs/rules/LX103.md) — Grid.ColumnSpan exceeds available columns ([#4])
- Category overview page: [layout](docs/rules/layout.md) ([#4])
- [LX104](docs/rules/LX104.md) — Grid definition shorthand not supported by target framework ([#4])
- `frameworkVersion` config field for opting into legacy framework targets; `DialectFeatures` helper for framework-gated capability detection ([#4])

## [0.2.0] - 2026-04-18

M2 — first content lint rules.

### Added

- [LX200](docs/rules/LX200.md) — SelectedItem binding should be TwoWay ([#3])
- [LX300](docs/rules/LX300.md) — x:Name should start with uppercase ([#3])
- [LX400](docs/rules/LX400.md) — Hardcoded string; use a resource ([#3])
- Category overview pages: [bindings](docs/rules/bindings.md), [naming](docs/rules/naming.md), [resources](docs/rules/resources.md) ([#3])

### Fixed

- [LX400](docs/rules/LX400.md): dropped a dead `WpfPresentation`-namespace branch in the attribute filter; unprefixed attributes remain the only in-scope form ([#3])

## [0.1.0] - 2026-04-18

M1 — plumbing end-to-end. Rule engine, CLI, config, plugin veneer, doc tooling, and test harness wired together with six tool/engine diagnostics.

### Added

- [LX001](docs/rules/LX001.md) — Malformed XAML ([#2])
- [LX002](docs/rules/LX002.md) — Unrecognized pragma directive ([#2])
- [LX003](docs/rules/LX003.md) — Malformed configuration ([#2])
- [LX004](docs/rules/LX004.md) — Cannot read file ([#2])
- [LX005](docs/rules/LX005.md) — Skipping non-XAML file ([#2])
- [LX006](docs/rules/LX006.md) — Internal error in rule ([#2])
- `xaml-lint lint` and `xaml-lint hook` CLI subcommands with `compact-json`, `sarif`, `msbuild`, and `pretty` formatters ([#2])
- `xaml-lint.config.json` discovery with `extends` presets (`xaml-lint:off`, `xaml-lint:recommended`, `xaml-lint:strict`) and per-file `overrides[]` ([#2])
- ReSharper-style suppression pragmas (`<!-- xaml-lint disable [once] RULE -->`, `<!-- xaml-lint restore RULE -->`) ([#2])
- Source-generated rule catalog (`IIncrementalGenerator`, `[XamlRule]` attribute) and `XamlLint.DocTool` for doc/schema/preset generation with `--check` CI mode ([#2])
- `XamlDiagnosticVerifier<TRule>` marker-based test harness with inline `[|…|]` and `{|LX###:…|}` spans ([#2])
- `PostToolUse` hook, `lint-xaml` skill, and `/xaml-lint:lint` slash command ([#2])
- `dotnet tool` packaging as `xaml-lint` ([#2])

[Unreleased]: https://github.com/XAML-Lint/xaml-lint/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/XAML-Lint/xaml-lint/compare/v0.5.0...v1.0.0
[0.5.0]: https://github.com/XAML-Lint/xaml-lint/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/XAML-Lint/xaml-lint/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/XAML-Lint/xaml-lint/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/XAML-Lint/xaml-lint/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/XAML-Lint/xaml-lint/releases/tag/v0.1.0
[#2]: https://github.com/XAML-Lint/xaml-lint/pull/2
[#3]: https://github.com/XAML-Lint/xaml-lint/pull/3
[#4]: https://github.com/XAML-Lint/xaml-lint/pull/4
[#5]: https://github.com/XAML-Lint/xaml-lint/pull/5
[#6]: https://github.com/XAML-Lint/xaml-lint/pull/6
