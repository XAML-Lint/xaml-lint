<img src="https://raw.githubusercontent.com/XAML-Lint/xaml-lint/main/assets/logo.png" alt="xaml-lint" width="128" height="128">

# xaml-lint

[![NuGet](https://img.shields.io/nuget/vpre/xaml-lint.svg)](https://www.nuget.org/packages/xaml-lint)
[![CI](https://github.com/XAML-Lint/xaml-lint/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/XAML-Lint/xaml-lint/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/XAML-Lint/xaml-lint.svg)](LICENSE)

A Claude Code plugin that lints XAML files for common issues, so Claude can catch XAML problems as it writes and edits code.

## Status

v0.5.0 — 20 rule IDs shipping: 6 tool/engine diagnostics (LX001–LX006) plus 14 analysis rules across Layout, Bindings, Naming, Resources, Input, and Deprecated categories. Rules are dialect-gated where the upstream semantics require it. Full catalog at [docs/rules/](docs/rules/); release history in [CHANGELOG.md](CHANGELOG.md).

### Platform support

Rule counts reflect how many of the 20 catalog IDs actually fire for each dialect — set `defaultDialect` in `xaml-lint.config.json` (or a `dialect="..."` file pragma) so dialect-specific rules gate correctly.

| Platform | Rules applying | Status |
| --- | --- | --- |
| WPF | 15 | Supported (primary target) |
| WinUI 3 | 18 | Supported (incl. `x:Bind`, `x:Uid`, `InputScope`, `MediaElement`) |
| UWP | 18 | Supported (incl. `x:Bind`, `x:Uid`, `InputScope`, `MediaElement`) |
| .NET MAUI | 16 | Supported (incl. `Slider`/`Stepper` range checks) |
| Avalonia | 14 | Dialect-agnostic rules only; no Avalonia-specific rules yet |
| Uno Platform | 14 | Dialect-agnostic rules only; no Uno-specific rules yet |

## Install

```
dotnet tool install -g xaml-lint
```

## Use

```
xaml-lint lint src/Views/MainView.xaml
xaml-lint lint "src/**/*.xaml"
```

Invoke as a plugin: install the plugin from the Claude Code marketplace (or `claude --plugin-dir ./path/to/this/repo` for dev). The `PostToolUse` hook fires on every `Write`/`Edit` of a `.xaml` file and runs `xaml-lint hook` to report diagnostics back to Claude automatically.

## Configure

Create `xaml-lint.config.json` at your repo root:

```json
{
  "$schema": "https://raw.githubusercontent.com/XAML-Lint/xaml-lint/main/schema/v1/config.json",
  "extends": "xaml-lint:recommended",
  "defaultDialect": "wpf",
  "rules": { "LX005": "off" }
}
```

See [docs/config-reference.md](docs/config-reference.md) for the full schema.

## Output formats

`xaml-lint lint --format <name>`:

- `pretty` — colored, TTY default.
- `compact-json` — stable JSON envelope; default when stdout is redirected; Claude's plugin hook reads this format.
- `msbuild` — one line per diagnostic; matches `dotnet build` output style.
- `sarif` — SARIF 2.1.0 for CI integrations.

## Exit codes

- `0` — no findings or only warning/info.
- `1` — at least one error-severity diagnostic.
- `2` — tool-level failure (malformed config, unreadable input, engine crash).

## Attribution

Many of the lint rules in this project are ports of checks from the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) by Matt Lacey, used under the MIT License. Ported rules carry the upstream `RXT###` code via their `UpstreamId` field and a source-file header comment. Tool/engine diagnostics (LX001–LX006) and some lint rules (e.g., LX104) are original to xaml-lint — their `UpstreamId` is null. The VS extension, code generation, and IDE-specific pieces of the original project are not part of this fork's scope. See [docs/comparison-with-rapid-xaml-toolkit.md](docs/comparison-with-rapid-xaml-toolkit.md) for the per-rule mapping.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the versioning policy, the "add a new rule" flow, and how to run tests locally.

## License

[MIT](LICENSE)
