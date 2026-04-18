# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.0.0](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Rule-level history is tracked in [AnalyzerReleases.Shipped.md](AnalyzerReleases.Shipped.md).

## [Unreleased]

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

[Unreleased]: https://github.com/jizc/xaml-lint/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/jizc/xaml-lint/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/jizc/xaml-lint/releases/tag/v0.1.0
[#2]: https://github.com/jizc/xaml-lint/pull/2
[#3]: https://github.com/jizc/xaml-lint/pull/3
