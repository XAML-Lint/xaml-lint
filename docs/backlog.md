# Backlog

Ideas considered for xaml-lint. No version commitments, no ordering. Pick freely when planning a release; check the dependency tier to know what infrastructure each idea would also need.

See [`release-policy.md`](release-policy.md) for how releases are gated.

## Rule ideas

Grouped by what new infrastructure (if any) the rule would require. IDs are proposed slots — categories are stable post-v1.2 rename, but assignment isn't a commitment.

### Tier 1 — No new infrastructure

Rules that drop straight into the existing single-file engine. No new options machinery, no resource resolution beyond what already exists.

| ID    | Category  | Summary                                                                                                       |
|-------|-----------|---------------------------------------------------------------------------------------------------------------|
| (TBD) | Usability | Button / `HyperlinkButton` / similar without `ICommand` or `Click` — interactive control with no behavior     |
| (TBD) | Usability | `ContentPage` / `Window` / `Page` without `Title` — identity-less surface                                     |
| (TBD) | Usability | `Setter` targeting a non-existent property — silently ignored                                                 |
| (TBD) | Usability | `ListView` / `CollectionView` without `ItemTemplate` — uses default, often degraded                           |
| (TBD) | Usability | `TabView` / `TabControl` with no items — empty navigation control                                             |
| (TBD) | Usability | Markup-extension misspellings that fall through to no-op — broader than LX0202/0203's dangling-reference scope|

### Tier 2 — Needs single-file resource index

Rules that walk a same-file `Resources` / `Style` graph. Same-file precursor to the cross-file resolver in tier 3.

| ID     | Category | Upstream | Summary                                                                                                                        |
|--------|----------|----------|--------------------------------------------------------------------------------------------------------------------------------|
| LX1000 | Styles   | #138     | `Setter` redeclares a property with the same value as a `BasedOn` parent style — redundant. Single-file scope. Opens the Styles category (LX10xx). |

### Tier 3 — Needs cross-file XAML resolver

Resolver walks `Resources` / `ResourceDictionary`, follows `MergedDictionaries` to other files, walks `App.xaml`. Theme-aware: resolves the same key against both light/dark dictionaries where a dialect supports them. Same symbol-table machinery an LSP needs for go-to-definition on `{StaticResource}` — build it once, every downstream rule benefits.

Implementation footprint when this enabler ships: diagnostic envelope likely gains an optional `relatedFiles: [...]` field on cross-file findings; config gains a workspace-root concept (new field or `--workspace-root` CLI flag); dogfood regimen updates so cross-file baselines run as whole-project sweeps rather than per-file.

| ID     | Category      | Upstream | Summary                                                                                                            |
|--------|---------------|----------|--------------------------------------------------------------------------------------------------------------------|
| LX0708 | Accessibility | RXT500   | Color contrast below WCAG threshold. Both themes, inherited backgrounds, `StaticResource` / `DynamicResource` resolved. |
| LX0403 | Resources     | #501     | `Color` vs `Brush` type mismatch at assignment sites                                                               |
| LX0404 | Resources     | #345     | `StaticResource` key typo (with did-you-mean from resolved key set)                                                |
| LX0801 | Platform      | #27      | Multi-file xmlns alias consistency                                                                                 |
| LX10xx | Styles        | #138     | Cross-file expansion of LX1000: same-value setter detection across `MergedDictionaries`. (Implicit-style usage detection from the same upstream issue still needs design — see Needs design.) |

### Tier 4 — Needs C# code-behind parsing

Rules that need to read `.xaml.cs` (Roslyn-based) to know what names/properties exist on the source side.

| ID     | Category | Upstream | Summary                                                                                                                                       |
|--------|----------|----------|-----------------------------------------------------------------------------------------------------------------------------------------------|
| LX0302 | Naming   | RXT #321 | Unused `x:Name`. Same-file XAML scan plus code-behind C# parsing. Reference-form notes preserved below.                                       |
| LX0204 | Bindings | —        | `{Binding}` to a readonly C# source property (no setter, no INPC) without `Mode=OneTime` — `OneWay`'s listener overhead with no payoff.       |

#### LX0302 reference-form research notes

When LX0302 returns, these are the reference forms the rule's `XamlNameReferenceScanner` originally tracked — preserved here so the analysis doesn't have to be rebuilt from scratch:

- **Markup-extension reference forms:** `{Binding ElementName=X}`, `{x:Reference X}`, `{x:Reference Name=X}`, `{Reference X}` (the unprefixed form is valid only when the XAML 2009 namespace is the default — common in MAUI XAML 2009 files).
- **Literal-attribute reference forms (WPF trigger / storyboard idioms):** attribute local name equals `TargetName` or `SourceName`, OR ends with `.TargetName` / `.SourceName` (attached-property syntax). Covers `Storyboard.TargetName`, `Setter.TargetName`, `Trigger.SourceName`, `Condition.SourceName`, `EventTrigger.SourceName`. The suffix test avoids matching unrelated names like `DataSourceName` by requiring the `.` separator before the suffix.
- **Same-namescope resolution** uses `XamlNameIndex.ResolveInScopeOf` — template boundaries (`ControlTemplate`, `DataTemplate`, `ItemsPanelTemplate`, `HierarchicalDataTemplate`) isolate names per scope.
- **Out of scope** (matches LX0202 / LX0203 posture): `{x:Bind path.Name}` typed paths, code-behind C# references (the open blocker until this tier's enabler ships).

## Needs design

Items where the trigger condition isn't clear yet, regardless of infrastructure. Promote into a tier when the design problem is solved.

- **Subjective margins / paddings** ([#324](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/324)) — no clear bright line for "doesn't scale."
- **Implicit-style detection** ([#138](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/138) follow-on) — when does a style warrant flagging, and how do we detect "implicit" reliably.
- **`IsRequiredForForm`** ([RXT #242](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/242) / [#137](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/137)) — no defined trigger; XAML structure can't infer "required."
- **`SizeOfSet` / `PositionInSet`** — framework auto-sets these on virtualizing list controls; manual lists are the wrong abstraction to flag.
- **`HeadingLevel` annotations** — every plausible trigger is a noisy heuristic.
- **`LX0901` nested scrollables** — needs per-dialect scrollable taxonomy (`ScrollViewer`, `ScrollView`, `CollectionView`, `ListView`, `TreeView`, `CarouselView`, …), axis awareness (`Orientation=`, `*ScrollBarVisibility=Disabled`), template-boundary suppression, and disabled-scroll detection. Belongs in Layout (`LX01xx`); promotes to tier 1 once designed.

## Infrastructure ideas (enablers)

Things to build that unlock multiple rules. Cross-checked against the rule entries above.

| Enabler                         | Unlocks                                                              | Notes                                                                                                                                              |
|---------------------------------|----------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| Per-rule options schema         | LX0300 `style` option, LX0400 widening knobs, LX0201 heuristic knobs | Config schema gains object form: `"LX####": { "severity": "...", "options": { ... } }`. String shorthand stays valid. Presets stay severity-only.  |
| Single-file resource index      | LX1000 (single-file)                                                 | Same-file `Resources` walk for `Style BasedOn` resolution. Precursor to cross-file resolver.                                                       |
| Cross-file XAML resolver        | All Tier 3 rules + LSP go-to-definition                              | Walks `Resources` / `ResourceDictionary` / `MergedDictionaries` / `App.xaml`; theme-aware.                                                        |
| Multi-file rule execution model | Tier 3 rules                                                         | Rules opt into multi-file by declaring dependencies; engine schedules re-runs when dependencies change. Single-file rules unaffected.              |
| C# code-behind parsing          | All Tier 4 rules                                                     | Roslyn-based. Same dependency LX0302 and LX0204 share.                                                                                             |
| LSP server                      | Editor integration on top of cross-file resolver                     | Min surface: rule execution + diagnostics + go-to-def + hover + code actions. Full LSP (rename, refactoring, formatting) out of scope.            |
| Workspace-root config concept   | Cross-file resolver                                                  | New config field or CLI flag (e.g. `--workspace-root`). Resolver needs a starting point beyond the file being linted.                              |
| Code-fix protocol               | Auto-fix hints in diagnostic envelope                                | Deferred — value over plain-text rule descriptions unproven for Claude as primary consumer. Revisit when LSP or human-facing CLI surface needs it. |

## Continuous, not batched

Drops in as dogfood surfaces issues — explicitly not items to "pick" from this backlog:

- **LX0400 attribute-list widening** as dogfood surfaces false negatives.
- **LX0201 `{Binding}` heuristic tuning** as dogfood surfaces noise.
- **Periodic Rapid-XAML-Toolkit upstream triage sweeps.** New triages get dated siblings alongside [`upstream-triage-2026-04-23.md`](upstream-triage-2026-04-23.md), not overwrites.
