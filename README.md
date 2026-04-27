![logo](https://raw.githubusercontent.com/XAML-Lint/xaml-lint/main/assets/logo-128.png)

# xaml-lint

[![NuGet](https://img.shields.io/nuget/v/xaml-lint.svg)](https://www.nuget.org/packages/xaml-lint)
[![CI](https://github.com/XAML-Lint/xaml-lint/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/XAML-Lint/xaml-lint/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/XAML-Lint/xaml-lint.svg)](LICENSE)

A XAML linter, with Claude Code plugin integration so Claude can catch XAML problems as it writes and edits code.

## Install

```
dotnet tool install -g xaml-lint
```

Requires the .NET 8, 9, or 10 SDK on `PATH`.

## Use with Claude Code

The plugin's `PostToolUse` hook shells out to the `xaml-lint` CLI, so the CLI must be installed and on `PATH` before the plugin can do anything:

```
dotnet tool install -g xaml-lint
xaml-lint --version
```

If `xaml-lint --version` fails, add the global tool directory to your `PATH` (`%USERPROFILE%\.dotnet\tools` on Windows, `~/.dotnet/tools` on macOS/Linux) and try again.

Then, inside Claude Code:

```
/plugin marketplace add XAML-Lint/xaml-lint
/plugin install xaml-lint@xaml-lint
```

The bundled `PostToolUse` hook runs `xaml-lint` on every `.xaml` / `.axaml` file Claude writes or edits and feeds diagnostics back into the conversation automatically. Use `/xaml-lint:lint <path-or-glob>` to trigger a manual lint.

## Use from the CLI

```
xaml-lint lint src/Views/MainView.xaml
xaml-lint lint "src/**/*.xaml"
```

Sample output:

```
src/Views/MainView.xaml
     8:20  warning LX0100  Grid.Row="5" but the enclosing Grid declares only 2 rows.
     8:33  info    LX0400  Hardcoded string on 'Text' should be moved to a resource.
     9:18  warning LX0300  x:Name 'userInput' should start with an uppercase letter.
```

## Configure

Create `xaml-lint.config.json` at your repo root:

```json
{
  "$schema": "https://raw.githubusercontent.com/XAML-Lint/xaml-lint/main/schema/v1/config.json",
  "extends": "xaml-lint:recommended",
  "defaultDialect": "wpf",
  "rules": { "LX0005": "off" }
}
```

See [docs/config-reference.md](docs/config-reference.md) for the full schema, and [docs/rules/](docs/rules/) for the full rule catalog (20 rules across Layout, Bindings, Naming, Resources, Input, and Usability categories, dialect-gated for WPF / WinUI 3 / UWP / MAUI / Avalonia / Uno).

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

Many of the lint rules in this project are ports of checks from the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) by Matt Lacey, used under the MIT License. Ported rules carry the upstream `RXT###` code via their `UpstreamId` field and a source-file header comment. Tool/engine diagnostics (LX0001–LX0006) and some lint rules (e.g., LX0104) are original to xaml-lint — their `UpstreamId` is null. The VS extension, code generation, and IDE-specific pieces of the original project are not part of this fork's scope. See [docs/comparison-with-rapid-xaml-toolkit.md](docs/comparison-with-rapid-xaml-toolkit.md) for the per-rule mapping.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the versioning policy, the "add a new rule" flow, and how to run tests locally.

## License

[MIT](LICENSE)
