---
name: lint-xaml
description: Use when the user asks to lint XAML, check a view, validate XAML markup, audit XAML for issues, or mentions XAML analysis. Invokes xaml-lint on the target file(s) and reports diagnostics.
---

# Lint XAML

Run `xaml-lint lint <path>` to analyze one or more XAML files. The tool emits compact JSON with an array of diagnostics. Each diagnostic has `ruleId`, `severity`, `message`, `file`, and start/end line+column fields.

## How to invoke

- Single file: `xaml-lint lint path/to/View.xaml`
- Multiple files / glob: `xaml-lint lint "src/**/*.xaml"`
- Explicit format: `xaml-lint lint --format compact-json path/to/View.xaml`

If `xaml-lint` isn't on PATH, check install with:
```
dotnet tool install -g xaml-lint
```

## Interpreting output

The envelope always has the shape:

```json
{
  "version": "1",
  "tool": { "name": "xaml-lint", "version": "<tool-version>" },
  "results": [ ... ]
}
```

Empty `results` means clean. For each diagnostic, read `ruleId`, `message`, `file`, and `startLine` to locate the issue. Use `helpUri` (when present) to look up the rule's documentation before deciding how to fix.

## Severity meaning

- `error` — almost certainly a bug; fix before shipping.
- `warning` — likely issue worth fixing; review case-by-case.
- `info` — suggestion; apply if it improves the code.

## Suppressing a diagnostic

If the diagnostic is a false positive in this specific spot, suppress with a XAML comment immediately before the offending element:

```xml
<!-- xaml-lint disable once LX300 -->
<Button x:Name="myButton" />
```

For broader suppression, edit `xaml-lint.config.json`:

```json
{ "rules": { "LX300": "off" } }
```
