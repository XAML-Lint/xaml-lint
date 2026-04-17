# xaml-lint

A Claude Code plugin that lints XAML files for common issues, so Claude can catch XAML problems as it writes and edits code.

## Status

v0.1.0 — engine, CLI, config, plugin, and test harness are all wired end-to-end. No content lint rules yet (those ship in v0.2+); v0.1.0 ships the six tool/engine diagnostics (LX001–LX006).

### Planned platform support

| Platform | Status |
| --- | --- |
| WPF | In progress |
| WinUI 3 | Planned |
| UWP | Planned |
| .NET MAUI | Planned |
| Avalonia | Planned |
| Uno Platform | Planned |

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
  "$schema": "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json",
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

The analysis rules in this project are derived from the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) by Matt Lacey, used under the MIT License. The VS extension, code generation, and IDE-specific pieces of the original project are not part of this fork's scope — only the XAML analysis. See [docs/comparison-with-rapid-xaml-toolkit.md](docs/comparison-with-rapid-xaml-toolkit.md) for the per-rule mapping.

## License

[MIT](LICENSE)
