# Comparison with Rapid XAML Toolkit

This project ports and re-implements the XAML analysis portion of the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) (Matt Lacey, MIT). The VS extension, view-model code generation, and IDE-specific pieces are out of scope — we only port the analyzers.

## Rule ID mapping

| xaml-lint | Upstream (RXT) | Notes |
|-----------|----------------|-------|
| LX001 | RXT999 | Malformed XAML — emitted by our parser, equivalent to RXT's catch-all malformed diagnostic. |
| LX002 | — | Unrecognized pragma directive. No RXT equivalent; our pragma grammar is new. |
| LX003 | — | Malformed configuration. Config format is new to xaml-lint. |
| LX004 | — | Cannot read file. Tool-level I/O diagnostic; no upstream. |
| LX005 | — | Skipping non-XAML file. Tool-level behavior; no upstream. |
| LX006 | — | Internal error in rule. Tool-level crash capture; no upstream. |

Lint-rule mappings for LX100+ land as those rules ship (v0.2 onward).

## Behavior differences

v0.1 only ports tool-level diagnostics and does not re-implement any analysis rules, so there are no behavior differences to note yet. This table grows with each rule release.

## Suppression model

Rapid XAML Toolkit uses `xaml-analysis` comments; `xaml-lint` uses `xaml-lint` comments with a fuller grammar (`disable once`, `restore` blocks, `All`). See the [suppression section of the design spec](superpowers/specs/2026-04-17-xaml-lint-design.md#34-suppression-pipeline-resharper-style) for the full grammar.
