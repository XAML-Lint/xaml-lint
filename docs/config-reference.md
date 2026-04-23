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
    { "files": "**/*.Designer.xaml", "rules": { "LX0400": "off" } }
  ],
  "rules": {
    "LX0300": "off",
    "LX0400": "warning",
    "LX0100": "error"
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
  - Shorthand: `"LX0100": "error"` — values are `off`, `info`, `warning`, `error`.
  - `"*": "off"` — resets every rule to the given severity (wildcard).

## Severity resolution order

For a given `(file, rule)`:

1. If the rule's declared `Dialects` doesn't include the detected dialect, the rule is **skipped** (not reported, not counted).
2. Start with the rule's `DefaultSeverity` (from its `[XamlRule]` attribute).
3. Apply the preset's `rules[ruleId]` — from `--preset <name>` if given, else from the config's `extends`, else the bundled `xaml-lint:recommended`.
4. Apply the config's top-level `rules[ruleId]`.
5. Apply the first matching `overrides[].rules[ruleId]`.
6. Apply CLI `--rule ID:<severity>` overrides (also the expansion of `--only`).
7. Inline `<!-- xaml-lint disable ... -->` pragmas can suppress diagnostics at the file/range level, unless `--no-inline-config` is set.

## Dialect detection cascade

Per file, earliest rule wins:

1. **Definitive xmlns sniff** — the root element's default namespace identifies the document unambiguously. MAUI (`http://schemas.microsoft.com/dotnet/2021/maui`) and Avalonia (`https://github.com/avaloniaui`) are detected here. A file that *is* a MAUI or Avalonia document is treated as such regardless of the invocation flag or config glob — the xmlns is ground truth, the CLI/config values are hints for disambiguating files whose xmlns is shared. Uno's MAUI-embedding feature produces MAUI-namespace files in an Uno repo; this rule keeps them from being linted as Uno.
2. CLI `--dialect <name>` flag.
3. First matching `overrides[].files` glob — its `dialect` applies.
4. Config-level `defaultDialect`.
5. Fallback: `wpf`.

WPF and UWP/WinUI 3 share `http://schemas.microsoft.com/winfx/2006/xaml/presentation` and cannot be distinguished from xmlns alone — files using that URL fall through to step 2 and rely on the invocation hint.
