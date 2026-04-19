# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.0.0](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Rule-level history is tracked in [AnalyzerReleases.Shipped.md](AnalyzerReleases.Shipped.md).

## [Unreleased]

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

- `CONTRIBUTING.md` with versioning policy and the "add a new rule" flow ([#6])
- Project branding: logo (`assets/logo.png`, `assets/logo.svg`), wired as NuGet `<PackageIcon>` — README and icon now ship at the nupkg root ([#6])
- Marketplace submission materials under `docs/marketplace/` (listing copy + screenshot spec for 3 PNGs) ([#6])

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
- `NumericRangeHelpers` in `XamlLint.Core` — literal-double parsing shared between LX501 and LX502 ([#5])

## [0.3.0] - 2026-04-18

M3 — Grid-family layout rules.

### Added

- [LX100](docs/rules/LX100.md) — Grid.Row without matching RowDefinition ([#4])
- [LX101](docs/rules/LX101.md) — Grid.Column without matching ColumnDefinition ([#4])
- [LX102](docs/rules/LX102.md) — Grid.RowSpan exceeds available rows ([#4])
- [LX103](docs/rules/LX103.md) — Grid.ColumnSpan exceeds available columns ([#4])
- Category overview page: [layout](docs/rules/layout.md) ([#4])
- `GridAncestryHelpers` and `LocationHelpers.GetElementNameSpan` in `XamlLint.Core` ([#4])
- [LX104](docs/rules/LX104.md) — Grid definition shorthand not supported by target framework ([#4])
- `frameworkVersion` config field for opting into legacy framework targets; `DialectFeatures` helper for framework-gated capability detection ([#4])

## [0.2.0] - 2026-04-18

M2 — first content lint rules.

### Added

- [LX200](docs/rules/LX200.md) — SelectedItem binding should be TwoWay ([#3])
- [LX300](docs/rules/LX300.md) — x:Name should start with uppercase ([#3])
- [LX400](docs/rules/LX400.md) — Hardcoded string; use a resource ([#3])
- Category overview pages: [bindings](docs/rules/bindings.md), [naming](docs/rules/naming.md), [resources](docs/rules/resources.md) ([#3])
- Meta-tests: `AnalyzerReleases` category column must match `XamlLintCategory.ForId`, and every rule must be linked from its category overview page ([#3])
- `MarkupExtensionHelpers` and `XamlNamespaces` / `LocationHelpers` in `XamlLint.Core` ([#3])

### Changed

- Plugin manifest version bumped to `0.2.0` to match the published tool and Nerdbank version ([#3])
- [LX300](docs/rules/LX300.md) docs now describe the casing check as Unicode-permissive (any `char.IsUpper` letter), matching the implementation ([#3])

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

[Unreleased]: https://github.com/XAML-Lint/xaml-lint/compare/v0.5.0...HEAD
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
