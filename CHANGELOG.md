# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Rule-level history is tracked in [AnalyzerReleases.Shipped.md](AnalyzerReleases.Shipped.md).

## [Unreleased]

### Added

- [LX0105](docs/rules/LX0105.md) — Zero-sized RowDefinition / ColumnDefinition (all dialects; warning in `:recommended`, error in `:strict`)
- [LX0106](docs/rules/LX0106.md) — Single-child Grid without row or column definitions (all dialects; off in `:recommended`, error in `:strict`)
- [LX0202](docs/rules/LX0202.md) — Binding ElementName target does not exist (all dialects; warning in `:recommended`, error in `:strict`)
- [LX0203](docs/rules/LX0203.md) — x:Reference target does not exist (all dialects; warning in `:recommended`, error in `:strict`)
- [LX0302](docs/rules/LX0302.md) — Unused x:Name (all dialects; off in `:recommended`, warning in `:strict`)

### Fixed

- Markup-extension parser now unquotes single- or double-quoted argument values (`{Binding ElementName='Foo'}`, `{x:Reference 'Foo'}`). Eliminates LX0202/LX0203 false positives against quoted-argument idioms; rules reading `NamedArguments` values (e.g. LX0200's `Mode=TwoWay` check) silently stop missing quoted forms too.

## [1.1.0] - 2026-04-23

### Added

- `--rule ID:severity` CLI flag for ad-hoc rule-severity overrides; repeatable, CSV-stackable, and also accepts an `--rule '{"ID":"severity"}'` object form mirroring the config's `rules:` schema
- `--preset recommended|strict|none` CLI flag to override the config's `extends:` per invocation
- `--no-inline-config` CLI flag to ignore `<!-- xaml-lint disable ... -->` pragmas
- `-c` short alias for `--config`
- [LX702](docs/rules/LX702.md) — TextBox lacks accessibility description (WPF/WinUI 3/UWP/Avalonia/Uno; off by default)
- [LX703](docs/rules/LX703.md) — Entry lacks accessibility description (MAUI; off by default)
- [LX800](docs/rules/LX800.md) — Uno platform XML namespace must be `mc:Ignorable`; opens the Platform category
- Scope-aware `XamlNameIndex` for `{x:Reference}` validation; templates isolate names from the outer scope, matching the XAML runtime

### Changed

- `--only` is now shorthand for `--preset none --no-config-lookup --rule ID:<severity>...` instead of a filter; bare IDs use the rule's `DefaultSeverity`. **Breaking.**
- `--no-config` renamed to `--no-config-lookup` to match eslint. **Breaking.**
- Dialect scope widened on seven rules per official-doc verification: [LX201](docs/rules/LX201.md), [LX301](docs/rules/LX301.md), [LX500](docs/rules/LX500.md), and [LX600](docs/rules/LX600.md) now apply to Uno; [LX501](docs/rules/LX501.md) now applies to Avalonia; [LX601](docs/rules/LX601.md) and [LX700](docs/rules/LX700.md) now apply to all dialects
- Dialect detection cascade puts definitive root-xmlns sniff (MAUI / Avalonia) ahead of `--dialect` and config: a document's declared namespace is ground truth, fixing ~418 false positives in Uno's `MauiEmbedding` samples where MAUI XAML was being linted under `--dialect uno`. **Breaking.**
- [LX700](docs/rules/LX700.md) / [LX701](docs/rules/LX701.md) — `AutomationProperties.LabeledBy="{x:Reference <name>}"` suppresses only when the target exists in the same XAML name scope; dangling or cross-template references now fire
- [LX700](docs/rules/LX700.md) / [LX701](docs/rules/LX701.md) / [LX702](docs/rules/LX702.md) — `{Binding ElementName=<name>}` is scope-validated the same way as `{x:Reference}`; dangling `ElementName` targets now fire
- [LX702](docs/rules/LX702.md) — reverse-direction WPF labeling via `<Label Target="{x:Reference <name>}">` or `<Label Target="{Binding ElementName=<name>}">` suppresses the diagnostic on the referenced `TextBox`, with template-scope isolation
- [LX501](docs/rules/LX501.md) / [LX502](docs/rules/LX502.md) — default severity promoted from `warning` to `error` to match upstream Rapid XAML Toolkit (RXT330/RXT335); an empty Min/Max range throws at runtime on UWP/WinUI and misbehaves on every other dialect. Pin to `"warning"` via the `rules` config to restore prior behavior
- [LX100](docs/rules/LX100.md)–[LX103](docs/rules/LX103.md) — `<Grid.RowDefinitions>` / `<Grid.ColumnDefinitions>` element syntax now takes precedence over the `RowDefinitions="..."` / `ColumnDefinitions="..."` shorthand attribute when a Grid declares both, matching upstream Rapid XAML Toolkit; Grids declaring only one form are unaffected

### Fixed

- [LX700](docs/rules/LX700.md) / [LX701](docs/rules/LX701.md) — MAUI's `SemanticProperties.Description` and `SemanticProperties.Hint` now suppress the rule, matching LX703
- [LX500](docs/rules/LX500.md), [LX503](docs/rules/LX503.md), [LX504](docs/rules/LX504.md), [LX505](docs/rules/LX505.md), [LX506](docs/rules/LX506.md) — recognise property-element syntax (`<TextBox.InputScope>`, `<Entry.Keyboard>`, `<Entry.IsPassword>`/`<Entry.MaxLength>`, `<Pin.Label>`, `<Slider.ThumbColor>`/`<Slider.ThumbImageSource>`), eliminating false positives on files that prefer the element form over attribute form
- [LX700](docs/rules/LX700.md) / [LX701](docs/rules/LX701.md) — `AutomationId` now suppresses the rule (matches upstream Rapid XAML Toolkit RXT350/RXT351); an image wired only through `AutomationId` no longer fires
- [LX800](docs/rules/LX800.md) — `http://uno.ui/not_win` added to the Uno platform-URI allowlist, matching upstream Rapid XAML Toolkit RXT700
- [LX301](docs/rules/LX301.md) — no longer flags UWP/WinUI `x:Uid` values in the `/ResourceFile/Key` resw namespace-scope form; only the trailing key segment is cased-checked
- Avalonia `.axaml` files are now linted as first-class XAML instead of always emitting [LX005](docs/rules/LX005.md) ("Skipping non-XAML file")
- [LX400](docs/rules/LX400.md) — values with no letters and no digits (Unicode PUA icon glyphs, UI-chrome punctuation like `"+"`/`":"`) are treated as non-localisable chrome and skipped
- `xaml-lint hook` no longer emits LX005 for every non-XAML file Claude edits; non-XAML paths short-circuit to an empty envelope before config/catalog load
- `xaml-lint hook` empty-envelope response now reports the actual tool version instead of the hardcoded `"dev"` literal

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

[Unreleased]: https://github.com/XAML-Lint/xaml-lint/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/XAML-Lint/xaml-lint/compare/v1.0.0...v1.1.0
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
