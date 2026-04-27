# xaml-lint — marketplace listing

## Short description (one line, ~120 chars)

A Claude Code plugin that lints XAML files — catch Grid-layout, binding, naming, and deprecation issues as code is written.

## Long description

`xaml-lint` analyzes XAML files and reports common problems, so Claude catches them as it writes and edits views.

The plugin installs a `PostToolUse` hook that runs `xaml-lint` on every `.xaml` / `.axaml` file Claude writes or edits; diagnostics land back in context automatically, without any prompt. A `/xaml-lint:lint` slash command and an on-demand skill handle the cases where you want to check files manually.

The rule catalog (20 IDs at v1) is derived from Matt Lacey's Rapid XAML Toolkit: Grid-layout sanity (`Grid.Row`/`Grid.Column` without matching definitions, spans exceeding available rows/columns), binding issues (`SelectedItem` should be `TwoWay`, prefer `x:Bind` on UWP/WinUI 3), naming (`x:Name` / `x:Uid` casing), resource-localization hints, input-control scope gaps, Slider/Stepper range checks, and deprecation warnings (`MediaElement` → `MediaPlayerElement`). Rules are dialect-gated; the WPF-primary, WinUI 3 / UWP / .NET MAUI rules only fire when those dialects are detected.

Output formats: `pretty` (ANSI, TTY default), `compact-json` (stable envelope; the hook emits this by default), `sarif` (SARIF 2.1.0 for CI), `msbuild` (one line per diagnostic, `dotnet build` style).

The analysis engine is stateless — no reflection, no MEF. A v2 LSP server is planned as a purely additive wrap.

## Feature bullets

- 14 analysis rules across Layout, Bindings, Naming, Resources, Input, and Usability categories, plus 6 tool diagnostics (LX0001–LX0006) — 20 rule IDs total at v1
- Dialect-aware: WPF primary, with dialect-gated rules for WinUI 3, UWP, and .NET MAUI
- ReSharper-style suppression pragmas (`<!-- xaml-lint disable [once] RULE -->`)
- Configurable via `xaml-lint.config.json` with three bundled presets (`xaml-lint:off`, `xaml-lint:recommended`, `xaml-lint:strict`)
- Four output formats: `pretty`, `compact-json`, `sarif`, `msbuild`
- Source-generated rule catalog; per-rule `HelpUri` pointing at hosted docs
- Published to NuGet as a `dotnet tool`

## Requirements

- Claude Code (plugin host)
- .NET 10 SDK on `PATH`
- `dotnet tool install -g xaml-lint` after plugin enable

## Links

- Repo: https://github.com/XAML-Lint/xaml-lint
- Docs: https://github.com/XAML-Lint/xaml-lint/tree/main/docs
- NuGet: https://www.nuget.org/packages/xaml-lint
- Changelog: https://github.com/XAML-Lint/xaml-lint/blob/main/CHANGELOG.md
