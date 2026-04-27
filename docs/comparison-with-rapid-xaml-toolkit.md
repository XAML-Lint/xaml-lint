# Comparison with Rapid XAML Toolkit

This project ports and re-implements the XAML analysis portion of the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) (Matt Lacey, MIT). The VS extension, view-model code generation, and IDE-specific pieces are out of scope — we only port the analyzers.

## Rule ID mapping

| xaml-lint | Upstream (RXT) | Notes |
|-----------|----------------|-------|
| LX0001 | RXT999 | Malformed XAML — emitted by our parser, equivalent to RXT's catch-all malformed diagnostic. |
| LX0002 | — | Unrecognized pragma directive. No RXT equivalent; our pragma grammar is new. |
| LX0003 | — | Malformed configuration. Config format is new to xaml-lint. |
| LX0004 | — | Cannot read file. Tool-level I/O diagnostic; no upstream. |
| LX0005 | — | Skipping non-XAML file. Tool-level behavior; no upstream. |
| LX0006 | — | Internal error in rule. Tool-level crash capture; no upstream. |
| LX0100 | RXT101 | Grid.Row without matching RowDefinition. Matches upstream semantics. Element syntax and the `RowDefinitions="..."` shorthand are both supported (the shorthand requires WPF .NET 10+, gated via `frameworkVersion` config). |
| LX0101 | RXT102 | Grid.Column without matching ColumnDefinition. Mirror of LX0100 for columns. |
| LX0102 | RXT103 | Grid.RowSpan exceeds available rows. Span considered in isolation — not combined with Grid.Row; out-of-range starting rows are reported by LX0100. |
| LX0103 | RXT104 | Grid.ColumnSpan exceeds available columns. Mirror of LX0102 for columns. |
| LX0104 | — | Grid definition shorthand not supported by target framework. xaml-lint-original; no upstream RXT equivalent. Fires on legacy-WPF (frameworkVersion < 10) when a Grid uses the `RowDefinitions="..."` or `ColumnDefinitions="..."` shorthand attribute. |
| LX0105 | — | Zero-sized RowDefinition / ColumnDefinition. xaml-lint-original; no upstream RXT equivalent. Literal-value check — `Height`/`Width` whose trimmed value is `0`, a negative number, or empty. Star-sized (including `0*`), `Auto`, positive numbers, and markup-extension values are skipped. Element-syntax `Height`/`Width` is out of scope for v1.2. |
| LX0106 | — | Single-child Grid without row or column definitions. xaml-lint-original; no upstream RXT equivalent. Fires on `<Grid>` with exactly one layout child and no declared `RowDefinitions` / `ColumnDefinitions` (either element or shorthand form). `DefaultEnabled=false`, `off` in `:recommended`, `error` in `:strict`. |
| LX0200 | RXT160 | SelectedItem binding should be TwoWay. Matches upstream; applies to all dialects where binding markup is used. |
| LX0201 | RXT170 | Prefer x:Bind over Binding. Scoped to UWP, WinUI 3, and Uno Platform — on those dialects `{x:Bind}` compiles to generated code and validates paths at build time. |
| LX0202 | RXT163 | Binding ElementName target does not exist. Applies to all dialects. Scope-aware — walks `ControlTemplate`/`DataTemplate`/`ItemsPanelTemplate`/`HierarchicalDataTemplate` boundaries the same way as LX0700–0702's LabeledBy validator. `DefaultEnabled=true`, `warning` in `:recommended`, `error` in `:strict`. |
| LX0203 | — | x:Reference target does not exist. Applies to all dialects. No upstream counterpart — upstream issue #502. Same scope semantics as LX0202. `DefaultEnabled=true`, `warning` in `:recommended`, `error` in `:strict`. |
| LX0300 | RXT452 | x:Name should start with uppercase. Matches upstream casing rule; unprefixed `Name` remains out of scope. |
| LX0301 | RXT451 | x:Uid should start with uppercase. UWP, WinUI 3, and Uno Platform; `x:Uid` has no runtime meaning on WPF. Mirror of LX0300 for `x:Uid`. |
| LX0400 | RXT200 | Hardcoded string. Our attribute-name list is deliberately conservative at v0.2; upstream's list is broader and will be matched as real-world false negatives surface. |
| LX0402 | RXT310 | Image Source filename invalid on Android. MAUI-only. URI skip list (`http://`, `https://`, `ms-appx:`, `ms-appdata:`, `file://`) is xaml-lint-specific — see Behavior differences. |
| LX0500 | RXT150 | TextBox lacks InputScope. UWP, WinUI 3, and Uno Platform — `InputScope` is a platform-specific hint that does not exist on WPF. Any literal or bound value suppresses the check. |
| LX0501 | RXT330 | Slider Minimum is greater than Maximum. WPF, MAUI, and Avalonia; UWP/WinUI raise a runtime exception on the same state (redundant there). Avalonia silently coerces Maximum up to Minimum — exactly the intent-vs-runtime mismatch the rule catches. Literal pair required; markup extensions suppress. |
| LX0502 | RXT335 | Stepper Minimum is greater than Maximum. MAUI-only control; same semantics as LX0501. |
| LX0503 | RXT300 | Entry lacks Keyboard. MAUI-only mirror of LX0500/RXT150; any literal or bound `Keyboard` value suppresses. |
| LX0504 | RXT301 | Password Entry lacks MaxLength. MAUI-only; fires only when `IsPassword="True"` is a literal (case-insensitive). Bound `IsPassword`, literal-false, or any present `MaxLength` suppresses. |
| LX0505 | RXT325 | Pin lacks Label. MAUI-only; rule is a guardrail against the `ArgumentException` the Maps control throws at runtime when a pin is added without a label. Any `Label` value (literal or bound) suppresses. |
| LX0506 | RXT331 | Slider sets both ThumbColor and ThumbImageSource. MAUI-only; presence of both attributes is the signal regardless of literal/bound values — see Behavior differences. |
| LX0601 | RXT320 | Line.Fill has no effect. Applies to all dialects — a `<Line>`'s geometry has zero interior area in WPF, WinUI 3, UWP, MAUI, Avalonia, and Uno, so `Fill` is a universal no-op. Presence of any `Fill` value on `<Line>` fires. |
| LX0602 | — | MAUI Shell nav-surface lacks Title and Icon. xaml-lint-original; no direct RXT equivalent (inspired by RXT issue #240). MAUI-only; fires on `<Tab>`, `<ShellContent>`, `<FlyoutItem>`, and `<MenuItem>` when neither `Title` nor `Icon` is set. Either alone (text-only or icon-only nav) suppresses; bound values count as set. `DefaultEnabled=true`, `warning` in `:recommended`, `error` in `:strict`. |
| LX0600 | RXT402 | MediaElement deprecated — use MediaPlayerElement. UWP, WinUI 3, and Uno Platform; WPF continues to ship `MediaElement` as its primary media control. |
| LX0700 | RXT350 | Image lacks accessibility description. Applies to all dialects — `AutomationProperties.Name`/`HelpText`/`LabeledBy` are supported across WPF, WinUI 3, UWP, MAUI, Avalonia, and Uno. Opens the Accessibility category. Off by default in `:recommended` — see Behavior differences. `IsInAccessibleTree` value-gated (literal `"False"` or bound suppresses; `"True"` does not). |
| LX0701 | RXT351 | ImageButton lacks accessibility description. MAUI-only; structural mirror of LX0700 for `ImageButton`. Same off-by-default stance and same `IsInAccessibleTree` gating. |
| LX0702 | RXT601 | TextBox lacks accessibility description. Applies to WPF/WinUI 3/UWP/Avalonia/Uno (MAUI is covered by LX0703). `DefaultEnabled=false`, `off` in `:recommended`, `warning` in `:strict`. `AutomationProperties.LabeledBy="{x:Reference <name>}"` requires the target to resolve in the same XAML name scope — dangling references fire. |
| LX0703 | — | Entry lacks accessibility description. MAUI-original sibling to LX0702; no upstream equivalent. `DefaultEnabled=false`, `off` in `:recommended`, `warning` in `:strict`. |
| LX0704 | — | Icon button lacks accessibility description. xaml-lint-original; no direct RXT equivalent (inspired by RXT242/137 issue threads but not a literal port). Applies to all dialects on `Button` and MAUI `ImageButton`. Fires on (a) symbolic `Content` per the LX0400 `IsSymbolOrGlyph` helper, (b) a single icon-class child (`Image`, `FontIcon`, `SymbolIcon`, `PathIcon`, `BitmapIcon`, `ImageIcon`, WPF `Path`), or (c) an empty button. Suppressor list matches LX0700–0703. `DefaultEnabled=false`, `off` in `:recommended`, `warning` in `:strict`. |
| LX0800 | RXT700 | Uno platform XML namespace must be `mc:Ignorable`. Opens the Platform category. Rule is a no-op when no Uno URIs are present, so it applies to all dialects. |

Lint-rule mappings continue to accrue as new categories ship.

## Behavior differences

- **LX0300 vs RXT452** — xaml-lint limits the casing check to `Name` in the XAML 2006
  (`http://schemas.microsoft.com/winfx/2006/xaml`) or 2009
  (`http://schemas.microsoft.com/winfx/2009/xaml`) namespace. Upstream historically treated
  the unprefixed `Name=` attribute the same way; we do not, because unprefixed `Name` in
  WPF is a framework-level convenience rather than a XAML-language identifier.
- **LX0400 vs RXT200** — upstream flags a wider set of text-presenting attribute names than
  our v0.2 scope (`Text`, `Title`, `Header`, `ToolTip`, `Content`, `PlaceholderText`,
  `Placeholder`, `Description`, `Watermark`). The list will grow as real projects surface
  false negatives.
- **Default-preset severities** — `xaml-lint:recommended` is deliberately quieter than
  upstream RXT for two rules that are noisy on typical real-world WPF codebases. Both are
  marked `DefaultEnabled = false` and written as `"off"` in `:recommended`: `LX0400`
  (hardcoded strings — most apps aren't fully localized, so the rule fires thousands of
  times on legitimately-intentional literals) and `LX0300` (lowercase `x:Name` is common
  for template-internal or pure-layout names; casing consistency is a team preference).
  RXT fires both at warning level by default. Users who want upstream-style strictness
  can extend `xaml-lint:strict`, where `LX0400` is `warning` and `LX0300` is `error`, or
  enable either rule explicitly in their own config.
- **LX0201 vs RXT170** — xaml-lint flags every `{Binding …}` attribute on UWP / WinUI 3 / Uno,
  with no heuristic for "is this form likely convertible to `{x:Bind}`?". The intent is a noisy
  informational signal that Claude and human reviewers can triage case-by-case; projects
  mid-migration typically suppress at the file or glob level.
- **LX0402 vs RXT310** — xaml-lint suppresses the rule when `Source`
  begins with a recognised URI scheme (`http://`, `https://`, `ms-appx:`,
  `ms-appdata:`, `file://`). Upstream RXT310's documentation does not
  enumerate such a list; our behavior is deliberate — those schemes are
  not local files and do not flow through the Android drawable pipeline.
- **LX0501/LX0502 vs RXT330/RXT335** — both xaml-lint and upstream require `Minimum` and
  `Maximum` to be literal parseable numbers; neither project fires when one value is bound.
  Dialect coverage differs: upstream's `SliderAnalyzer`/`StepperAnalyzer` gate on
  Xamarin.Forms/MAUI only, while xaml-lint also covers WPF and Avalonia (Avalonia silently
  coerces Maximum up to Minimum — exactly the intent-vs-runtime mismatch the rule catches).
  UWP/WinUI raise a runtime exception in this state, so upstream omits them as redundant.
  Severity matches upstream: `Error` by default; downgrade to `warning` via `rules` config
  if the runtime-throw cases on UWP/WinUI make the promotion too aggressive.
- **LX0506 vs RXT331** — xaml-lint fires on any combination of literal and bound values
  for `ThumbColor` and `ThumbImageSource`; presence of both attributes is the signal.
  Upstream documentation does not specify the literal/bound combination behavior; we
  chose the inclusive reading because MAUI's precedence (`ThumbImageSource` wins) is the
  same regardless of whether either value is resolved at runtime.
- **LX0700 / LX0701 default-preset stance** — both accessibility rules are
  `DefaultEnabled = false` and appear as `"off"` in `:recommended` (same pattern as LX0300
  and LX0400). Accessibility-complete codebases are rare; enabling these by default floods
  output on typical MAUI projects. Teams who want to enforce a11y should extend `:strict`
  (where both rules fire at `warning`) or enable them explicitly.
- **LX0700 / LX0701 vs RXT350 / RXT351** — `AutomationProperties.IsInAccessibleTree` is
  treated as value-gated rather than presence-only: only a literal `"False"`
  (case-insensitive) or a bound value suppresses. A literal `"True"` reasserts default
  inclusion, so the control is in the accessibility tree and still requires a name —
  treating that as a suppressor would hide a real a11y gap. The other three escape
  attributes (`Name`, `HelpText`, `LabeledBy`) suppress on any value.
- **LX0700 / LX0701 `LabeledBy` tightening** — starting in this release,
  `AutomationProperties.LabeledBy="{x:Reference <name>}"` suppresses the
  rule only when `<name>` resolves in the same XAML name scope as the
  image. Dangling references now fire. Non-reference literals and other
  markup extensions (`{Binding …}`) continue to suppress as before.
- **LX0702 vs RXT601** — xaml-lint validates `{x:Reference}` targets
  against a scope-aware name index (`ControlTemplate`/`DataTemplate`/
  `ItemsPanelTemplate`/`HierarchicalDataTemplate` each open a nested
  scope). Upstream RXT601 accepts any `LabeledBy` value on presence
  alone, so a typo'd reference passes upstream but fires here.
- **LX0800 vs RXT700** — applied to all dialects rather than a
  hypothetical `Dialect.Uno` gate; the rule is a structural no-op when
  no Uno URIs are present, so universal application has no cost. The
  Uno URI allowlist is hardcoded (no config knob) and matches Uno
  Platform 6.0 as of 2026-04. Two additional differences:
  - **URI matching.** xaml-lint matches Uno URIs by canonical namespace
    URI. Upstream `UnoIgnorablesAnalyzer` compares against a literal
    string with a one-slash typo (`http:/uno.ui/...`) and would only
    fire when an author reproduces the same typo. Our behavior is
    intentional.
  - **Ignorable-token matching.** xaml-lint splits `mc:Ignorable` into
    a whitespace-tokenized set and checks exact membership. Upstream
    uses `ignorable.StringValue.Contains(alias)` — a substring check
    that would suppress `android` when `mc:Ignorable="androidx"` is
    present. Our set-membership check rejects those false positives.
- **LX0200 vs RXT160** — xaml-lint parses binding named arguments
  (`Mode=TwoWay`) rather than searching for the literal substring
  `"TwoWay"` anywhere in the value; we therefore do not get fooled by
  values like `ConverterParameter=TwoWayMode`. We also restrict firing
  to `{Binding}` and `{x:Bind}` extensions, whereas upstream fires on
  any markup extension starting with `{`.
- **LX0301 vs RXT451** — xaml-lint case-checks only the final segment of
  resw namespace-scope `/File/Key` references (so `x:Uid="/Strings/MyKey"`
  passes here because `MyKey` starts uppercase). Upstream checks
  `value[0]` unconditionally and would flag the leading `/`.
- **LX0400 symbol handling vs RXT200** — upstream gates on
  `char.IsLetterOrDigit(value[0])` — a first-character test. xaml-lint
  walks every rune looking for at least one localisable character, so
  values like `"+ Add"` or `"$ 5.00"` fire here (the prose/digit segment
  is localisable) but not upstream. Intentional: mixed symbol-plus-prose
  strings still need translation.
- **LX0400 element/attribute model vs RXT200** — xaml-lint fires on any
  element carrying a matching attribute name. Upstream is per-processor:
  only specific (element, attribute) pairs fire (`TextBlock.Text`,
  `AppBarButton.Label`, `ToggleSwitch.OnContent/OffContent`,
  `MenuFlyoutItem.Text`, `ToolTipService.ToolTip`, etc.). Ours is
  broader; the cost is offset by LX0400 being `"off"` in `:recommended`.
- **LX0402 vs RXT310** — upstream's regex validates the full `Source`
  string (rejecting uppercase anywhere in the path); xaml-lint extracts
  the leaf filename and validates that only, because Android drawables
  can't live in subdirectories anyway. Upstream permits leading digits
  and leading underscores; xaml-lint rejects both to match aapt2's
  resource-name rules. Upstream treats `\` as invalid; xaml-lint splits
  on both `\` and `/` and validates the leaf segment, tolerating
  author-written Windows paths.
- **LX0501/LX0502 vs RXT330/RXT335 dialect scope** — see the LX0501/LX0502
  entry above; xaml-lint's WPF and Avalonia coverage is an intentional
  extension beyond upstream's Xamarin/MAUI gate.
- **LX0600 / LX0601 fix hints** — upstream emits structured fix data
  (`AnalysisActions.AndAddAttribute` for LX0600's MediaElement →
  MediaPlayerElement migration; `RemoveAttribute` for LX0601's redundant
  `Fill`). xaml-lint emits diagnostics only; a code-fix surface is out
  of scope until the plugin-level actions protocol is designed.
- **LX0700 / LX0701 `AutomationId`** — both rules treat `AutomationId`
  as a presence-only suppressor (added in this release). Matches
  upstream RXT350/RXT351; previously our rules would fire when an
  image was wired only through `AutomationId`.
- **LX0701 vs RXT351 added checks** — xaml-lint adds
  `AutomationProperties.IsInAccessibleTree` value-gating (mirrors LX0700)
  and `SemanticProperties.Description`/`Hint` as suppressors; upstream
  RXT351 recognises neither. Both are deliberate strictness improvements.
- **LX0702 vs RXT601 dialect scope** — upstream RXT601 fires on UWP and
  WinUI only. xaml-lint widens to WPF, Avalonia, and Uno, where the
  same `AutomationProperties` surface is available; record the
  narrower upstream scope here so the extension is explicit.
- **Grid shorthand counting vs upstream** — our `CountCommaSeparated`
  filters whitespace-only tokens, so `RowDefinitions=",,Auto,,"` yields
  1 and `""` yields 0. Upstream uses `value.Split(',').Length` verbatim
  (3 and 1 respectively). Empty tokens aren't real `RowDefinition`
  entries, so the filtering is deliberate.

For rules that remain unported from upstream and the rationale behind
deferring them, see
[`unported-upstream-rules.md`](unported-upstream-rules.md).

## Suppression model

Rapid XAML Toolkit uses `xaml-analysis` comments; `xaml-lint` uses `xaml-lint` comments with a fuller grammar (`disable once`, `restore` blocks, `All`). Every per-rule doc under [`docs/rules/`](rules/) has a "How to suppress violations" section with copy-pasteable pragma snippets.
