# Roadmap

Last updated: 2026-04-23.

xaml-lint's plan from v1.1.0 (current) forward. Covers the next minor (v1.2) and major (v2.0) releases, plus a short tail of post-v2 directions that are scoped but not committed.

## Framing

- **Terminal state:** v2.0 — when RXT500 (color contrast) ships on top of an LSP-era cross-file resolver. Everything past that is "considered but not committed."
- **Ordering principle:** infrastructure dependency. v1.2 is bounded by "single-file, deterministic." v2.0 is bounded by "needs a resource graph." Nothing in v1.2 blocks on v2.0; v2.0 uses infrastructure that does not exist yet.
- **Cadence bias:** fewer, larger themed releases. Release admin (changelog, version bumps, NuGet publish, dogfood sweeps, release notes) dominates the marginal shipping cost, so the default is one tight minor (v1.2) before the LSP major (v2.0), not a chain of small minors.
- **No calendar commitments.** Sequence only. Release windows are "when ready + dogfood clean."
- **Dogfood gate per release.** Each version bump runs the corpus sweep from [`dogfooding.md`](dogfooding.md) for each dialect whose rules changed. Baseline diff is the gate, not "did tests pass."

## Rule ID scheme — rename in v1.2

Today's `LX###` runs out of first-digit category slots with Shell: digits 0 (Tool), 1 (Layout), 2 (Bindings), 3 (Naming), 4 (Resources), 5 (Input), 6 (Usability), 7 (Accessibility), 8 (Platform), 9 (Shell) → all 10 used. Adding Styles or Migration (both on the post-v2 list) has nowhere to go.

v1.2 migrates every rule ID from `LX###` to `LX####`, with the first two digits encoding the category. Existing rules keep their visible digits — the migration is "insert a `0` after `LX`":

| Category      | Old        | New           |
|---------------|------------|---------------|
| Tool          | LX001–006  | LX0001–0006   |
| Layout        | LX100–104  | LX0100–0104   |
| Bindings      | LX200–201  | LX0200–0201   |
| Naming        | LX300–301  | LX0300–0301   |
| Resources     | LX400, 402 | LX0400, LX0402|
| Input         | LX500–506  | LX0500–0506   |
| Usability     | LX600–601  | LX0600–0601   |
| Accessibility | LX700–703  | LX0700–0703   |
| Platform      | LX800      | LX0800        |

**Design advantages over alternatives:**

- **Numeric throughout.** No letter-prefix mnemonic (avoids a `LXA`/`LXL`/`LXB` scheme).
- **100 categories** (00–99) of 100 rules each — 10,000 rule slots total; ~500× current count.
- **Lossless for existing IDs.** LX100 is visibly the same rule as LX0100. Docs, test markers, pragmas migrate mechanically.
- **Matches Roslyn/StyleCop/FxCop convention.** 4-digit IDs are the analyzer-ecosystem norm.
- **Sorts lexicographically.** `LX0100 < LX0700 < LX1000` matches semantic ordering.

**Rename scope (mechanical but thorough):**

- All 20 rule source files + their tests
- All 20 rule doc files + category pages
- `comparison-with-rapid-xaml-toolkit.md`, `unported-upstream-rules.md`, `upstream-triage-2026-04-23.md`, `dogfooding.md`
- `AnalyzerReleases.Shipped.md`
- Preset JSON (`xaml-lint:off`, `:recommended`, `:strict`)
- Config schema and examples
- Test markers (`{|LX###:…|}` → `{|LX####:…|}`)
- Pragmas in sample XAML
- CHANGELOG entries (reference form only; historical entries retain old IDs in their release notes)

## v1.2 — "Catalog completion + rename"

Ships: rename + additive rules + code-fix protocol.

### New rules (11)

| ID      | Category      | Upstream  | Summary                                                              |
|---------|---------------|-----------|----------------------------------------------------------------------|
| LX0105  | Layout        | #429      | Zero-sized `RowDefinition`/`ColumnDefinition`. Literal-only.         |
| LX0106  | Layout        | #114      | Single-child `Grid` with no Row/ColumnDefinitions. Off by default.   |
| LX0202  | Bindings      | #383      | Invalid `{Binding ElementName=…}`. Reuses LX0702's `XamlNameIndex`.  |
| LX0203  | Bindings      | #502      | Invalid `{x:Reference …}`. Reuses LX0702's `XamlNameIndex`.          |
| LX0302  | Naming        | #321      | Unused `x:Name` (XAML-only mode — no code-behind reach).             |
| LX0704  | Accessibility | #242/#137 | Image-only button lacks accessible description.                      |
| LX0705  | Accessibility | #242/#137 | Form element lacks `IsRequiredForForm` (WinUI/UWP/Uno).              |
| LX0706  | Accessibility | #242/#137 | List-item lacks `SizeOfSet`/`PositionInSet` (WinUI/UWP/Uno).         |
| LX0707  | Accessibility | #242/#137 | Missing heading-level annotation.                                    |
| LX0900  | Shell         | #240      | MAUI Shell `Tab`/`ShellContent` lacks `Title`/`Icon`. Opens category.|
| LX0901  | Shell         | #510      | Nested scrollable elements. Per-dialect scrollable taxonomy.         |

### Infrastructure / protocol changes

- **Rule ID rename** (see previous section).
- **Code-fix protocol.** Add optional `fixes: [{title, edits: [...]}]` field to the diagnostic envelope. Non-breaking additive output-shape change. Each output format (`pretty`, `compact-json`, `msbuild`, `sarif`) gets a minimal surface for the new field.
- **Fix hints emitted.** LX0600 (`MediaElement` → `MediaPlayerElement` attribute swap) and LX0601 (remove redundant `Fill`). Closes the open delta called out in `comparison-with-rapid-xaml-toolkit.md`.
- **Generalized `XamlNameIndex`.** LX0702's scope-aware name index becomes a shared analysis primitive consumed by LX0202/LX0203/LX0700–0703. No user-visible surface.

### Out of scope for v1.2

- Cross-file resource resolution (v2.0).
- RXT500 color contrast — any form, including literal-only. Deferred per [`unported-upstream-rules.md`](unported-upstream-rules.md) recommendation ("partial implementations are more likely to cause noise than the full version").
- Code-fix hints for rules beyond LX0600/LX0601 — add opportunistically as each rule's fix shape becomes obvious.

## v2.0 — "LSP era"

Architectural inflection. Introduces the infrastructure several deferred rules need.

### Infrastructure

- **Cross-file resource resolver.** Walks `Resources`/`ResourceDictionary`, follows `MergedDictionaries` to other files, walks `App.xaml` for app-level defaults. Theme-aware: resolves the same key against both light/dark dictionaries where a dialect supports them. This is the same symbol-table machinery an LSP needs for "go to definition" on a `{StaticResource}` reference — we build it once and every downstream rule benefits.
- **LSP server.** Minimum surface needed to expose rule execution + cross-file resolution to editors. Go-to-definition, hover, and code actions come along as a bonus because the resolver work unlocks them. A full LSP implementation (formatting, rename, refactoring) is out of scope.
- **Multi-file rule execution model.** Rules opt into multi-file by declaring dependencies. Engine schedules re-runs when dependencies change. Single-file rules (v1.2 and earlier) unaffected.
- **Workspace-root config concept.** Resolver needs a starting point beyond the file being linted. New config field or CLI flag (exact shape TBD during v2.0 design).

### Rules unlocked

| Planned ID | Upstream issue | Summary                                                                    |
|------------|----------------|----------------------------------------------------------------------------|
| LX0708     | RXT500         | Color contrast below WCAG threshold. Full version — both themes, inherited backgrounds, `StaticResource`/`DynamicResource` resolved. Categorized as Accessibility (contrast is a WCAG a11y rule, not a resource-plumbing one). |
| LX0403     | #501           | `Color` vs `Brush` type mismatch at assignment sites.                      |
| LX0404     | #345           | `StaticResource` key typo (with did-you-mean from resolved key set).       |
| LX0801     | #27            | Multi-file xmlns alias consistency.                                        |

### Breaking-change footprint

- Diagnostic envelope gains optional `relatedFiles: [...]` field on cross-file findings.
- Config gains `workspaceRoot` (or equivalent) concept.
- Dogfood regimen updates: cross-file baselines require whole-project sweeps rather than per-file.
- CLI may grow a `--workspace-root` flag.

## Post-v2 — considered, not committed

- **`LX10xx` — Styles category.** Upstream #138 (style duplication, implicit-style usage). Multi-file by nature; follows v2's resolver. Needs its own category design — "when does a style warrant flagging, how do we detect 'implicit' reliably?"
- **`LX11xx` — Migration category.** RXT401's redefined form (flag Xamarin.Forms `Checked`/`Unchecked` on MAUI `CheckBox` — "you meant `CheckedChanged`"). Absorbs any future dialect-migration warnings.
- **Subjective, deferred indefinitely:** #324 margins/paddings that don't scale. No clear bright line; parks in the backlog.
- **Continuous, not batched:** LX0400 attribute-list widening, LX0201 `{Binding}` heuristic tuning. Drop in when dogfood surfaces false negatives/noise. Not roadmap milestones.
- **Upstream firehose:** re-run the Rapid-XAML-Toolkit triage sweep periodically. New triages get dated siblings alongside [`upstream-triage-2026-04-23.md`](upstream-triage-2026-04-23.md), not overwrites.

Post-v2 category slots `LX12xx`–`LX99xx` remain free.

## Version / release policy

- **v1.2** = rule ID rename + additive rules + code-fix protocol. The rename is technically breaking but ships as a minor: while xaml-lint has no users, breaking changes don't force a major bump.
- **v2.0** = LSP + cross-file resolver + RXT500 and siblings. Major bump signals architectural inflection, not a semver-breaking policy.
- **Post-v2 minors (v2.1, v2.2, …)** land additive rules under existing categories. New categories (Styles, Migration) may open in post-v2 minors.
- **Dogfood sweep is the release gate.** Any unexpected delta on the corpus is a blocker; only intentional, explained deltas ship.
- **Semver posture.** Pre-adoption, breaking changes ship as minors. When adoption materializes, this tightens to strict semver and breaking changes force majors again.
