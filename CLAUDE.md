# CLAUDE.md

## Project overview

`xaml-lint` is a Claude Code plugin that analyzes XAML files and reports problems. The goal is for Claude to invoke it during development to catch XAML issues as code is written and edited — the primary consumer is Claude itself, not a human running a CLI.

The analysis rules are derived from the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) (MIT, Matt Lacey). Code generation, VSIX-specific pieces, and the IDE extension surface from the original project are out of scope — we only port/reimplement the analysis.

## Scope

- **In scope**: XAML static analysis, rule execution, structured output for machine consumption, Claude Code plugin wiring.
- **Out of scope**: code generation from view models, IDE tooling, refactoring commands, UI surfaces.

## Target platforms

Initial focus: **WPF**. Architecture should keep other XAML dialects in mind so we don't paint ourselves into a corner:

- WinUI 3, UWP, .NET MAUI, Avalonia, Uno Platform (all planned)

Rule implementations should be explicit about which dialect(s) they apply to rather than assuming WPF is universal.

## Conventions

Project is greenfield — conventions will be added here as decisions get made. Don't invent rules that aren't written down; ask before introducing a new pattern.

## Attribution reminders

- Keep Matt Lacey's copyright line in `LICENSE` whenever that file is touched.
- When porting code or rules from Rapid XAML Toolkit, preserve any existing attribution comments in the source files.
