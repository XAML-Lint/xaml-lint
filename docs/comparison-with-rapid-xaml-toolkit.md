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
| LX100 | RXT101 | Grid.Row without matching RowDefinition. Matches upstream semantics. Element syntax and the `RowDefinitions="..."` shorthand are both supported (the shorthand requires WPF .NET 10+, gated via `frameworkVersion` config). |
| LX101 | RXT102 | Grid.Column without matching ColumnDefinition. Mirror of LX100 for columns. |
| LX102 | RXT103 | Grid.RowSpan exceeds available rows. Span considered in isolation — not combined with Grid.Row; out-of-range starting rows are reported by LX100. |
| LX103 | RXT104 | Grid.ColumnSpan exceeds available columns. Mirror of LX102 for columns. |
| LX104 | — | Grid definition shorthand not supported by target framework. xaml-lint-original; no upstream RXT equivalent. Fires on legacy-WPF (frameworkVersion < 10) when a Grid uses the `RowDefinitions="..."` or `ColumnDefinitions="..."` shorthand attribute. |
| LX200 | RXT160 | SelectedItem binding should be TwoWay. Matches upstream; applies to all dialects where binding markup is used. |
| LX201 | RXT170 | Prefer x:Bind over Binding. Scoped to UWP/WinUI 3 per upstream semantics — on those dialects `{x:Bind}` compiles to generated code and validates paths at build time. |
| LX300 | RXT452 | x:Name should start with uppercase. Matches upstream casing rule; unprefixed `Name` remains out of scope. |
| LX301 | RXT451 | x:Uid should start with uppercase. UWP/WinUI 3 only; `x:Uid` has no runtime meaning on WPF. Mirror of LX300 for `x:Uid`. |
| LX400 | RXT200 | Hardcoded string. Our attribute-name list is deliberately conservative at v0.2; upstream's list is broader and will be matched as real-world false negatives surface. |
| LX500 | RXT150 | TextBox lacks InputScope. UWP/WinUI 3 only — `InputScope` is a platform-specific hint that does not exist on WPF. Any literal or bound value suppresses the check. |
| LX501 | RXT330 | Slider Minimum is greater than Maximum. WPF and MAUI only; UWP/WinUI raise a runtime exception on the same state, so static analysis is redundant there. Literal pair required — markup extensions on either attribute suppress the check. |
| LX502 | RXT335 | Stepper Minimum is greater than Maximum. MAUI-only control; same semantics as LX501. |
| LX600 | RXT402 | MediaElement deprecated — use MediaPlayerElement. UWP/WinUI 3 only; WPF continues to ship `MediaElement` as its primary media control. |

Lint-rule mappings continue to accrue as new categories ship.

## Behavior differences

- **LX300 vs RXT452** — xaml-lint limits the casing check to `Name` in the XAML 2006
  (`http://schemas.microsoft.com/winfx/2006/xaml`) or 2009
  (`http://schemas.microsoft.com/winfx/2009/xaml`) namespace. Upstream historically treated
  the unprefixed `Name=` attribute the same way; we do not, because unprefixed `Name` in
  WPF is a framework-level convenience rather than a XAML-language identifier.
- **LX400 vs RXT200** — upstream flags a wider set of text-presenting attribute names than
  our v0.2 scope (`Text`, `Title`, `Header`, `ToolTip`, `Content`, `PlaceholderText`,
  `Placeholder`, `Description`, `Watermark`). The list will grow as real projects surface
  false negatives.
- **LX201 vs RXT170** — xaml-lint flags every `{Binding …}` attribute on UWP/WinUI 3, with
  no heuristic for "is this form likely convertible to `{x:Bind}`?". The intent is a noisy
  informational signal that Claude and human reviewers can triage case-by-case; projects
  mid-migration typically suppress at the file or glob level.
- **LX501/LX502 vs RXT330/RXT335** — xaml-lint requires both attributes to be literal
  numbers before firing. Upstream Rapid XAML Toolkit also flags the case when only one
  attribute is literal and the other is bound; we defer that until the false-positive rate
  on real projects is known.

## Suppression model

Rapid XAML Toolkit uses `xaml-analysis` comments; `xaml-lint` uses `xaml-lint` comments with a fuller grammar (`disable once`, `restore` blocks, `All`). See the [suppression section of the design spec](superpowers/specs/2026-04-17-xaml-lint-design.md#34-suppression-pipeline-resharper-style) for the full grammar.
