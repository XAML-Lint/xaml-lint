# Upstream issue triage — 2026-04-23

Point-in-time scan of open issues in the [Rapid XAML Toolkit](https://github.com/mrlacey/Rapid-XAML-Toolkit) repository, filtered for relevance to xaml-lint's scope (XAML static analysis only — no VSIX, code generation, or editor tooling). Companion to [`unported-upstream-rules.md`](unported-upstream-rules.md), which inventories the named `RXT###` rules we have not ported; this doc surveys community-suggested rule *ideas* that never became named RXT rules but may still be worth porting.

## Bugs affecting existing rules

**None found.** Every open upstream bug concerns Visual Studio / VSIX infrastructure rather than rule semantics:

- VS freezes when the extension is installed ([#492](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/492))
- BuildAnalysis does not work on .NET 6 WPF projects ([#518](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/518))
- Warnings duplicated in the Error List ([#508](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/508), [#509](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/509))

Our comparison doc already records every known intentional behavioral difference per rule; no open upstream bug describes a defect in a rule we've ported.

## New rule candidates — strong fit

Ordered by implementation cost (low → medium). All four are single-file, deterministic, and fit existing infrastructure.

| # | Upstream | Idea | Why it fits now |
|---|----------|------|-----------------|
| 1 | [#383](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/383) | Invalid `{Binding ElementName=Foo}` | Reuses `XamlNameIndex` from LX0702. The scope-aware name resolver already validates `AutomationProperties.LabeledBy="{x:Reference …}"`. |
| 2 | [#502](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/502) | Invalid `{x:Reference}` anywhere | Same infrastructure as #383. Upstream specifically calls out the unhelpful `TargetInvocationException` produced at runtime when the target is missing. |
| 3 | [#429](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/429) | `<RowDefinition Height="0"/>` / width < 1 | Literal-only check; slots next to LX0100–LX0104 in the Layout category. No new infrastructure. |
| 4 | [#114](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/114) | `<Grid>` with single child and no `Row/ColumnDefinitions` | Structural single-file check. Likely noisy — would want `DefaultEnabled=false` and `off` in `:recommended` (same pattern as LX0300/LX0400/LX0700/LX0701). |

**Recommendation.** Ship #383 + #502 as a paired "name-reference validation" sub-category. They're essentially free extensions of the LX0702 work that just landed, they catch real runtime crashes, and they reuse our scope-aware name index so there's no new infrastructure debt.

## Candidates needing design work

Reasonable ideas, but each wants a round of brainstorming before implementation.

- [#240](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/240) — MAUI Shell `Tab` needs `Title`/`Icon`. Structural mirror of LX0700/LX0701. Would open a Shell sub-category.

## Defer to v2 / LSP work

These share RXT500's cross-file-resolution blocker (see [`unported-upstream-rules.md`](unported-upstream-rules.md)) or are otherwise stylistic/subjective:

- [#321](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/321) —
  unused `x:Name`. Tractable but produces unacceptable false positives
  without code-behind C# awareness (every code-behind-reached name
  reports as unused). Defer until C# parsing infrastructure exists,
  alongside LSP work. The XAML-only reference-form taxonomy is preserved
  in [`backlog.md`](backlog.md)'s tier 4 research notes.
- [#501](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/501) — Color vs Brush type mismatch. Needs cross-file resource resolution.
- [#345](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/345) — StaticResource key typo detection. Same blocker.
- [#138](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/138) — style duplication / implicit-style usage. An entire category we haven't started; overlaps with [#320](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/320)'s roadmap board.
- [#27](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/27) — inconsistent namespace aliases across files. Multi-file.
- [#324](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/324) — margins/paddings that don't scale well. Subjective; hard bright line.
- [#146](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/146), [#320](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/320) — meta "ideas wanted" boards; mine periodically.
- [#179](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/179) — extend InputScope *suggested actions*. Upstream is about suggesting specific values in a code-fix surface; we don't emit fix hints yet (see the LX0600/LX0601 note in the comparison doc).

## Considered, not committed

Items that may eventually become rules but need real design work that
v2's planned infrastructure does not unblock. Not on any release path.

- [#242](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/242) +
  [#137](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/137) —
  the accessibility expansion. The "image-only button" piece shipped
  in v1.2 as LX0704. The remaining ideas (form `IsRequiredForForm`,
  list-item `SizeOfSet`/`PositionInSet`, missing `HeadingLevel`) all
  fail the trigger-condition test: there's no XAML-structural way to
  know which fields are "required", which manual lists need
  position-of metadata, or which TextBlocks are headings. Each needs a
  real design pass before it could ship.
- [#510](https://github.com/mrlacey/Rapid-XAML-Toolkit/issues/510) —
  nested scrollable elements. Tractable as a structural check but
  requires per-dialect scrollable taxonomy, axis awareness, template-
  boundary suppression, and disabled-scroll detection. Belongs in
  Layout (`LX01xx`) when designed. Original v1.2 implementation
  deferred during the audit.

## Out of scope

Per [`CLAUDE.md`](../CLAUDE.md), we port only analyzer rules. The following open issues are IDE/VSIX/generation/tooling and do not apply:

VS2026 support (#527), template VSIX (#526), doc link review (#524), Editor-Extras ideas (#523), color-format markup (#522), WinGet DSC (#520), code-fix for child elements (#519), project principles (#517), NuGet README (#516), error-list redundancy (#509), custom-analyzer duplication (#508), custom-analyzer triggering (#505), Custom XAML Analysis for VS2022 (#504), VS freeze (#492), reproducible builds (#490), tabs vs spaces (#487), VS2022 support (#481), telemetry GUIDs (#478), ViewCell analyzer (#472), WebView analyzer for Xamarin.Forms (#471), ITrackingSpan highlighting (#466), design-time data generation (#463), ThreadedWaitDialog (#461), resx/resw encoding (#458), Blend integration (#456), file checkout (#454), markup-extension template (#452), attached-property template (#451), benchmarking (#442), StyleCop→EditorConfig (#420), ProjectType in BuildAnalysis (#409), SDK-style projects (#408), Symbol Visualiser markup extensions (#401), BuildAnalysis dependencies (#392), XamarinForms grid syntax conversion (#381), extract XAML style for Xamarin.Forms (#379), document cache invalidation (#375), nuspec file list (#358), Intellisense enhancements (#346), custom analyzers for comments (#339), suppress-by-element-type (#338 — our pragma system already covers the equivalent), wildcard code matches in settings (#330), uid resource visualisation (#323), VSIX localization (#312), EditorConfig migration (#292), generation docs (#267), StackLayout→Grid conversion (#261), Surround With (#207), extract styles (#205), per-project generation profiles (#153), multi-doc analysis (#136), file/project-level fixes (#133), Xliff localization (#82).

## Methodology note

This triage used the GitHub CLI (`gh issue list --repo mrlacey/Rapid-XAML-Toolkit --state open --limit 100`) against the upstream repository on 2026-04-23. 70 open issues at the time of scan. Upstream activity is sparse — most recent non-meta activity is from early 2025 — so this snapshot should stay usable for a while. If upstream sees a burst of new issues, re-run the sweep and drop a new dated file beside this one rather than overwriting.

The 2026-04-27 v1.2 audit pass re-bucketed RXT #321 (to "Defer to v2 / LSP work") and RXT #242 + #137 + #510 (to "Considered, not committed"); the original "Candidates needing design work" classification overstated readiness for those items.
