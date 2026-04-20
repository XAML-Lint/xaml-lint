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
| LX201 | RXT170 | Prefer x:Bind over Binding. Scoped to UWP, WinUI 3, and Uno Platform — on those dialects `{x:Bind}` compiles to generated code and validates paths at build time. |
| LX300 | RXT452 | x:Name should start with uppercase. Matches upstream casing rule; unprefixed `Name` remains out of scope. |
| LX301 | RXT451 | x:Uid should start with uppercase. UWP, WinUI 3, and Uno Platform; `x:Uid` has no runtime meaning on WPF. Mirror of LX300 for `x:Uid`. |
| LX400 | RXT200 | Hardcoded string. Our attribute-name list is deliberately conservative at v0.2; upstream's list is broader and will be matched as real-world false negatives surface. |
| LX402 | RXT310 | Image Source filename invalid on Android. MAUI-only. URI skip list (`http://`, `https://`, `ms-appx:`, `ms-appdata:`, `file://`) is xaml-lint-specific — see Behavior differences. |
| LX500 | RXT150 | TextBox lacks InputScope. UWP, WinUI 3, and Uno Platform — `InputScope` is a platform-specific hint that does not exist on WPF. Any literal or bound value suppresses the check. |
| LX501 | RXT330 | Slider Minimum is greater than Maximum. WPF, MAUI, and Avalonia; UWP/WinUI raise a runtime exception on the same state (redundant there). Avalonia silently coerces Maximum up to Minimum — exactly the intent-vs-runtime mismatch the rule catches. Literal pair required; markup extensions suppress. |
| LX502 | RXT335 | Stepper Minimum is greater than Maximum. MAUI-only control; same semantics as LX501. |
| LX503 | RXT300 | Entry lacks Keyboard. MAUI-only mirror of LX500/RXT150; any literal or bound `Keyboard` value suppresses. |
| LX504 | RXT301 | Password Entry lacks MaxLength. MAUI-only; fires only when `IsPassword="True"` is a literal (case-insensitive). Bound `IsPassword`, literal-false, or any present `MaxLength` suppresses. |
| LX505 | RXT325 | Pin lacks Label. MAUI-only; rule is a guardrail against the `ArgumentException` the Maps control throws at runtime when a pin is added without a label. Any `Label` value (literal or bound) suppresses. |
| LX506 | RXT331 | Slider sets both ThumbColor and ThumbImageSource. MAUI-only; presence of both attributes is the signal regardless of literal/bound values — see Behavior differences. |
| LX601 | RXT320 | Line.Fill has no effect. Applies to all dialects — a `<Line>`'s geometry has zero interior area in WPF, WinUI 3, UWP, MAUI, Avalonia, and Uno, so `Fill` is a universal no-op. Presence of any `Fill` value on `<Line>` fires. Placed in the Deprecated category alongside superseded-API rules; category scope widened to cover "no runtime effect" markup. |
| LX600 | RXT402 | MediaElement deprecated — use MediaPlayerElement. UWP, WinUI 3, and Uno Platform; WPF continues to ship `MediaElement` as its primary media control. |
| LX700 | RXT350 | Image lacks accessibility description. Applies to all dialects — `AutomationProperties.Name`/`HelpText`/`LabeledBy` are supported across WPF, WinUI 3, UWP, MAUI, Avalonia, and Uno. Opens the Accessibility category. Off by default in `:recommended` — see Behavior differences. `IsInAccessibleTree` value-gated (literal `"False"` or bound suppresses; `"True"` does not). |
| LX701 | RXT351 | ImageButton lacks accessibility description. MAUI-only; structural mirror of LX700 for `ImageButton`. Same off-by-default stance and same `IsInAccessibleTree` gating. |
| LX702 | RXT601 | TextBox lacks accessibility description. Applies to WPF/WinUI 3/UWP/Avalonia/Uno (MAUI is covered by LX703). `DefaultEnabled=false`, `off` in `:recommended`, `warning` in `:strict`. `AutomationProperties.LabeledBy="{x:Reference <name>}"` requires the target to resolve in the same XAML name scope — dangling references fire. |
| LX703 | — | Entry lacks accessibility description. MAUI-original sibling to LX702; no upstream equivalent. `DefaultEnabled=false`, `off` in `:recommended`, `warning` in `:strict`. |
| LX800 | RXT700 | Uno platform XML namespace must be `mc:Ignorable`. Opens the Platform category. Rule is a no-op when no Uno URIs are present, so it applies to all dialects. |

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
- **Default-preset severities** — `xaml-lint:recommended` is deliberately quieter than
  upstream RXT for two rules that are noisy on typical real-world WPF codebases. Both are
  marked `DefaultEnabled = false` and written as `"off"` in `:recommended`: `LX400`
  (hardcoded strings — most apps aren't fully localized, so the rule fires thousands of
  times on legitimately-intentional literals) and `LX300` (lowercase `x:Name` is common
  for template-internal or pure-layout names; casing consistency is a team preference).
  RXT fires both at warning level by default. Users who want upstream-style strictness
  can extend `xaml-lint:strict`, where `LX400` is `warning` and `LX300` is `error`, or
  enable either rule explicitly in their own config.
- **LX201 vs RXT170** — xaml-lint flags every `{Binding …}` attribute on UWP / WinUI 3 / Uno,
  with no heuristic for "is this form likely convertible to `{x:Bind}`?". The intent is a noisy
  informational signal that Claude and human reviewers can triage case-by-case; projects
  mid-migration typically suppress at the file or glob level.
- **LX402 vs RXT310** — xaml-lint suppresses the rule when `Source`
  begins with a recognised URI scheme (`http://`, `https://`, `ms-appx:`,
  `ms-appdata:`, `file://`). Upstream RXT310's documentation does not
  enumerate such a list; our behavior is deliberate — those schemes are
  not local files and do not flow through the Android drawable pipeline.
- **LX501/LX502 vs RXT330/RXT335** — xaml-lint requires both attributes to be literal
  numbers before firing. Upstream Rapid XAML Toolkit also flags the case when only one
  attribute is literal and the other is bound; we defer that until the false-positive rate
  on real projects is known.
- **LX506 vs RXT331** — xaml-lint fires on any combination of literal and bound values
  for `ThumbColor` and `ThumbImageSource`; presence of both attributes is the signal.
  Upstream documentation does not specify the literal/bound combination behavior; we
  chose the inclusive reading because MAUI's precedence (`ThumbImageSource` wins) is the
  same regardless of whether either value is resolved at runtime.
- **LX700 / LX701 default-preset stance** — both accessibility rules are
  `DefaultEnabled = false` and appear as `"off"` in `:recommended` (same pattern as LX300
  and LX400). Accessibility-complete codebases are rare; enabling these by default floods
  output on typical MAUI projects. Teams who want to enforce a11y should extend `:strict`
  (where both rules fire at `warning`) or enable them explicitly.
- **LX700 / LX701 vs RXT350 / RXT351** — `AutomationProperties.IsInAccessibleTree` is
  treated as value-gated rather than presence-only: only a literal `"False"`
  (case-insensitive) or a bound value suppresses. A literal `"True"` reasserts default
  inclusion, so the control is in the accessibility tree and still requires a name —
  treating that as a suppressor would hide a real a11y gap. The other three escape
  attributes (`Name`, `HelpText`, `LabeledBy`) suppress on any value.
- **LX700 / LX701 `LabeledBy` tightening** — starting in this release,
  `AutomationProperties.LabeledBy="{x:Reference <name>}"` suppresses the
  rule only when `<name>` resolves in the same XAML name scope as the
  image. Dangling references now fire. Non-reference literals and other
  markup extensions (`{Binding …}`) continue to suppress as before.
- **LX702 vs RXT601** — xaml-lint validates `{x:Reference}` targets
  against a scope-aware name index (`ControlTemplate`/`DataTemplate`/
  `ItemsPanelTemplate`/`HierarchicalDataTemplate` each open a nested
  scope). Upstream RXT601 accepts any `LabeledBy` value on presence
  alone, so a typo'd reference passes upstream but fires here.
- **LX800 vs RXT700** — applied to all dialects rather than a
  hypothetical `Dialect.Uno` gate; the rule is a structural no-op when
  no Uno URIs are present, so universal application has no cost. The
  Uno URI allowlist is hardcoded (no config knob) and matches Uno
  Platform as of 2026-04.

For rules that remain unported from upstream and the rationale behind
deferring them, see
[`unported-upstream-rules.md`](unported-upstream-rules.md).

## Suppression model

Rapid XAML Toolkit uses `xaml-analysis` comments; `xaml-lint` uses `xaml-lint` comments with a fuller grammar (`disable once`, `restore` blocks, `All`). Every per-rule doc under [`docs/rules/`](rules/) has a "How to suppress violations" section with copy-pasteable pragma snippets.
