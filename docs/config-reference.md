# Configuration reference

`xaml-lint` reads configuration from `xaml-lint.config.json`. The file is discovered by walking up from each linted file toward the repo root; the first match wins. If no project config is found, the tool falls back to a user-global config at `%APPDATA%/xaml-lint/config.json` (Windows) or `~/.config/xaml-lint/config.json` (Unix).

## Full shape

```json
{
  "$schema": "https://raw.githubusercontent.com/XAML-Lint/xaml-lint/main/schema/v1/config.json",
  "extends": "xaml-lint:recommended",
  "defaultDialect": "wpf",
  "frameworkVersion": "10",
  "overrides": [
    { "files": "src/legacy/**/*.xaml", "frameworkVersion": "9" },
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

## Field reference

- `$schema` — URL of the JSON Schema for autocomplete in VS Code / Rider.
- `extends` — one of `xaml-lint:off`, `xaml-lint:recommended`, `xaml-lint:strict`. Preset severities are applied first; the local `rules` block overrides.
- `defaultDialect` — one of `wpf`, `winui3`, `uwp`, `maui`, `avalonia`, `uno`. Required.
- <a id="frameworkversion"></a>`frameworkVersion` (optional) — major version of the target framework. Accepted forms: `"10"`, `"10.0"`, `"net10.0"` (case-insensitive). Used by rules that gate behavior on framework support — most prominently the WPF-on-.NET-10 grid definition shorthand. Omit to assume the newest framework (current behavior). Set on a per-`overrides[]` entry to apply to a subset of files (e.g., a legacy folder).
- `overrides[]` — each entry:
  - `files` — gitignore-style glob.
  - `dialect` (optional) — overrides `defaultDialect` for matching files.
  - `frameworkVersion` (optional) — overrides root `frameworkVersion` for matching files.
  - `rules` (optional) — overrides severities for matching files.
- `rules` — map from rule ID to severity:
  - Shorthand: `"LX100": "error"` — values are `off`, `info`, `warning`, `error`.
  - `"*": "off"` — resets every rule to the given severity (wildcard).

## Severity resolution order

For a given `(file, rule)`:

1. If the rule's declared `Dialects` doesn't include the detected dialect, the rule is **skipped** (not reported, not counted).
2. Start with the rule's `DefaultSeverity` (from its `[XamlRule]` attribute).
3. Apply the preset's `rules[ruleId]` from `extends`.
4. Apply the config's top-level `rules[ruleId]`.
5. Apply the first matching `overrides[].rules[ruleId]`.

CLI flags (`--only`, etc.) are applied last; their precedence will be documented when severity-promotion flags land in a later release.

## Dialect detection cascade

Per file:

1. CLI `--dialect <name>` flag — always wins.
2. First matching `overrides[].files` glob — its `dialect` applies.
3. Config-level `defaultDialect`.
4. Xmlns sniff — MAUI (`http://schemas.microsoft.com/dotnet/2021/maui`) and Avalonia (`https://github.com/avaloniaui`) are detected from the root element's default namespace. WPF and UWP/WinUI 3 share the `winfx/2006/xaml/presentation` URL and cannot be distinguished from xmlns alone.
5. Fallback: `wpf`.
