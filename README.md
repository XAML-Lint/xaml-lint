# xaml-lint

A Claude Code plugin that lints XAML files for common issues, so Claude can catch XAML problems as it writes and edits code.

## Status

Early development. Initial focus is **WPF**, but the architecture is being designed with other XAML dialects in mind.

### Planned platform support

| Platform       | Status     |
| -------------- | ---------- |
| WPF            | In progress |
| WinUI 3        | Planned    |
| UWP            | Planned    |
| .NET MAUI      | Planned    |
| Avalonia       | Planned    |
| Uno Platform   | Planned    |

## What it does

Analyzes XAML files and reports problems — things like missing accessibility attributes, misuse of styling/resources, deprecated patterns, and dialect-specific pitfalls. Designed to run non-interactively so Claude Code can invoke it during development.

## Attribution

The analysis rules in this project are derived from the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) by Matt Lacey, used under the MIT License. The VS extension, code generation, and IDE-specific pieces of the original project are not part of this fork's scope — only the XAML analysis.

## License

[MIT](LICENSE)
