# Unported upstream rules

This document is a forward-looking companion to
[`comparison-with-rapid-xaml-toolkit.md`](comparison-with-rapid-xaml-toolkit.md).
That doc maps what is already ported; this one inventories what **isn't**, and
explains why — so the next person deciding what to pull in can do so with eyes
open.

As of the v1.1 working tree (post-remaining-upstream-rules batch), the upstream Rapid XAML
Toolkit corpus of 25 analyzer rules (plus the `RXTPOC` experimental marker)
breaks down as:

- **20 ported** — see the mapping table in
  [`comparison-with-rapid-xaml-toolkit.md`](comparison-with-rapid-xaml-toolkit.md).
- **1 unported (this document)** — RXT500.
- **1 deliberately dropped** — RXT401 (CheckBox event pair); see rationale
  below.
- **1 experimental marker ignored** — `RXTPOC` is a proof-of-concept stub in
  upstream, not a rule.

**v1.1 update:** RXT601 and RXT700 were ported in the remaining-upstream
batch (as [LX0702](rules/LX0702.md) and [LX0800](rules/LX0800.md)), together
with a MAUI-only accessibility sibling ([LX0703](rules/LX0703.md) — not
an upstream port) and a retrofit of LX0700/LX0701 to validate
`AutomationProperties.LabeledBy` targets via the same `XamlNameIndex`
infrastructure LX0702 introduces. See
[`comparison-with-rapid-xaml-toolkit.md`](comparison-with-rapid-xaml-toolkit.md)
for the current ported-rule mapping.

## Summary of the one remaining rule

| Upstream | Title                  | Effort | Primary blocker                                               |
|----------|------------------------|--------|---------------------------------------------------------------|
| RXT500   | Color contrast warning | Hard   | Cross-file resource resolution, theme awareness, luminance math |

## RXT500 — Color contrast warning

> The luminosity of the foreground and background colors is below the
> recommended level of 4.5:1.
>
> — [upstream RXT500 doc](https://github.com/mrlacey/Rapid-XAML-Toolkit/blob/main/docs/warnings/RXT500.md)

**What it checks.** For every element with a `Foreground` (or text-facing)
brush and a resolvable `Background`, compute the [WCAG 2.0 relative luminance
ratio](https://www.w3.org/TR/2008/REC-WCAG20-20081211/#visual-audio-contrast-contrast)
and flag pairs below 4.5:1 (or 3:1 for large text — upstream does not
distinguish).

### Why this is the hard one

1. **Color parsing is the easy part.** XAML accepts several literal forms:
   `#RRGGBB`, `#AARRGGBB`, `#RGB`, `#ARGB`, named colors (`Red`, `Gold`),
   and in some dialects `Color.FromHex("…")`. A parser is ~100 lines.

2. **Resource resolution is the load-bearing part.** Real codebases almost
   never inline colors on the leaf element. They use:
   - `Foreground="{StaticResource PrimaryTextBrush}"`
   - `Background="{DynamicResource LayerBackground}"`

   Resolving those references means:
   - Walking up the element tree to find a `Resources`/`ResourceDictionary`
     that defines the key.
   - Following `MergedDictionaries` entries — which commonly reach into other
     files via `Source="…/Colors.xaml"`.
   - Walking `App.xaml`'s resource tree for application-level defaults.

   None of this is possible today. `XamlDocument` is single-file; rule
   execution has no cross-file symbol table.

3. **Theme awareness.** WinUI, MAUI, and UWP all ship light/dark theme
   dictionaries. The same key (`SystemBaseHighColor`) evaluates to different
   colors per theme. A faithful RXT500 port would need to check both themes
   and flag any pair that fails in either.

4. **Background is inherited.** When a `<TextBlock>` has no `Background`,
   the visual tree inherits from the nearest ancestor with one. The rule
   engine has to walk parents until it finds a resolved value — which may
   itself be a `StaticResource`.

5. **Markup extensions on brushes.** `{Binding ForegroundBrush}` can't be
   evaluated statically. The rule has to decide whether to skip or flag;
   upstream skips, and we'd follow suit.

6. **Non-brush colors.** `<TextBlock.Foreground><SolidColorBrush …/>` as
   element syntax. Gradient brushes (`LinearGradientBrush`) where contrast
   depends on position. The rule engine would need a "pick the worst stop"
   heuristic — non-trivial and debatable.

### What it would take to ship

The honest answer: a **cross-file resource resolver** that pulls together
app-level and element-level dictionaries into a lookup that each rule can
query. This is roughly the infrastructure an LSP server would need anyway —
the same symbol-table machinery that makes "go to definition" work on a
`{StaticResource}` reference is what RXT500 needs to evaluate colors.
Building it once unlocks RXT500 and likely makes several future rules
cheaper too.

Near-term incremental scope that could land without a full resolver:
- **Literal-only mode.** Flag only pairs where both `Foreground` and
  `Background` are inline literal colors. Skips resource references,
  skips inheritance. Small, shippable, catches a minority of real cases.
- **File-scoped resolver.** Resolve `{StaticResource}` references within
  the same XAML file only (including any inline `<ResourceDictionary>`
  blocks). Covers more cases but still misses the common
  `App.xaml`-defined keys.

Either incremental path still requires WCAG luminance math. The math itself
is a one-time ~60-line helper (`relativeLuminance`, `contrastRatio`).

**My recommendation.** Defer RXT500 until the LSP work lands in v2. The
partial implementations are more likely to cause noise (false negatives feel
like "oh, the rule didn't catch this real issue") than the full version.

## Dropped: RXT401 — handle both Checked and Unchecked events

Upstream flags a `CheckBox` that handles exactly one of `Checked` /
`Unchecked` but not the other. **MAUI's `CheckBox` has neither event** —
it exposes a single `CheckedChanged` event, consolidating what in
Xamarin.Forms was two separate events. A literal port against
`Dialect.Maui` would match zero real-world XAML.

The options that were considered during brainstorming:

- **Port literally.** Rule never fires in MAUI-era code. Dead code.
- **Redefine as a migration aid.** Flag any `Checked` or `Unchecked`
  event handler on `<CheckBox>` in a MAUI-dialect document ("these
  events don't exist here — did you mean `CheckedChanged`?"). Could
  catch incompletely migrated Xamarin.Forms code.
- **Drop.** Accept that the rule's semantic has no MAUI analogue.

**We chose drop.** Maintaining a dead rule for completeness' sake isn't
worth the catalog noise. If migration aids become a theme later (e.g.,
LX8xx "Migration"), RXT401's redefined form would fit there.

## Prioritization guidance

Only RXT500 remains. It's the hard one and should wait for the v2 LSP
work — the cross-file resource resolver LSP needs is the same
infrastructure RXT500 needs to evaluate `{StaticResource}`-bound
colors against theme dictionaries.
