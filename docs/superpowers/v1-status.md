# xaml-lint — v1 status tracker

| | |
|---|---|
| **Status** | In progress |
| **Updated** | 2026-04-19 |
| **Spec** | [2026-04-17-xaml-lint-design.md](specs/2026-04-17-xaml-lint-design.md) |
| **Current release** | [v0.5.0](../../CHANGELOG.md#050---2026-04-19) |

A single-page snapshot of what's shipped against the v1 design and what remains before a v1.0.0 tag. Keep it current when milestones graduate or §13 open items close.

## Milestone progress

| Milestone | Version | Status | Notes |
|---|---|---|---|
| M0 — Scaffold | — | ✅ Done | Solution, `Directory.Build.props`, `Directory.Packages.props`, `version.json`, CI across win/linux/mac |
| M1 — Plumbing end-to-end | [v0.1.0](https://github.com/jizc/xaml-lint/releases/tag/v0.1.0) | ✅ Done | Engine, CLI, config, plugin veneer, test harness, doc tool, six tool diagnostics |
| M2 — Easy rules | [v0.2.0](https://github.com/jizc/xaml-lint/releases/tag/v0.2.0) | ✅ Done | LX200, LX300, LX400 |
| M3 — Grid family | [v0.3.0](https://github.com/jizc/xaml-lint/releases/tag/v0.3.0) | ✅ Done | LX100–LX103 plus bonus LX104 (framework-version-gated shorthand) |
| M4 — Dialect-gated rules | [v0.4.0](https://github.com/jizc/xaml-lint/releases/tag/v0.4.0) | ✅ Done | LX201, LX301, LX500, LX501, LX502, LX600 |
| M5 — Pre-v1 polish | v0.5.0 | ✅ Done | Repo transfer + owner-only URL migration, `CONTRIBUTING.md`, NuGet readme+icon (pre-M5 via branding commit), marketplace materials. See [spec](specs/2026-04-18-m5-pre-v1-polish-design.md). |
| **v1.0.0 release** | **v1.0.0** | 🚧 Remaining | Gates: M5 complete, alpha suffix dropped from version.json, announcement drafted. |

## Rule catalog (20 IDs)

Spec targeted 19 IDs (6 tool + 13 lint); we shipped 20 after adding LX104 in M3. All live in [`AnalyzerReleases.Shipped.md`](../../AnalyzerReleases.Shipped.md); `Unshipped.md` is empty.

| Category | IDs | Status |
|---|---|---|
| Tool (LX0xx) | LX001–LX006 | ✅ All shipped (v0.1.0) |
| Layout (LX1xx) | LX100, LX101, LX102, LX103, LX104 | ✅ All shipped (v0.3.0) |
| Bindings (LX2xx) | LX200, LX201 | ✅ All shipped (v0.2.0, v0.4.0) |
| Naming (LX3xx) | LX300, LX301 | ✅ All shipped (v0.2.0, v0.4.0) |
| Resources (LX4xx) | LX400 | ✅ Shipped (v0.2.0) |
| Input (LX5xx) | LX500, LX501, LX502 | ✅ All shipped (v0.4.0) |
| Deprecated (LX6xx) | LX600 | ✅ Shipped (v0.4.0) |

All rules have per-rule docs under [`docs/rules/`](../rules/), every category has an overview page, and the RXT comparison page is current.

## M5 deliverables

Direct mapping of M5 spec §2 — each row is one gate.

| # | Item | Phase | Status | Notes |
|---|---|---|---|---|
| 1 | Transfer repo from `jizc/` to `XAML-Lint` org | A | ✅ Done | Transferred via `gh api POST /repos/jizc/xaml-lint/transfer`; local remote updated; 5 in-repo owner references flipped (plugin manifest, Directory.Build.props, CHANGELOG compare links, SARIF InfoUri, LX006 bug-report link). |
| 2 | Migrate owner slug across all HelpUri + `$schema` URLs | B | ✅ Done | 26 in-source HelpUri sites + 2 DocTool URL constants + 4 regenerated schema/preset JSONs + 2 user-facing `$schema` refs. URL shape preserved — GitHub Pages explored and declined. |
| 3 | Write `CONTRIBUTING.md` | C | ✅ Done | Versioning policy (spec §10) + "add a new rule" flow + how to run tests + DocTool usage. Linked from README. |
| 4 | NuGet package readme + icon | C | ✅ Done | Completed pre-M5 via branding commit 98fe56d; verified via `dotnet pack` — nupkg includes `logo.png` and `README.md` at archive root. |
| 5 | Plugin marketplace submission materials | C | ✅ Done | Listing copy + screenshot spec under `docs/marketplace/`. Submission itself deferred to v1.0.0. |

## Other deferred items (post-v1, tracked for visibility)

From spec §12 — none of these block v1.0.0.

- LSP server (v2 headliner; engine is already stateless-by-design to make the wrap additive).
- Auto-fix (`--fix` flag; rule-declared `Fix()` methods).
- `--error-on` / `--warning-on` severity promotion flags.
- Config merging across discovery levels (currently first-match-wins).
- Automatic `dotnet tool install` on plugin enable.
- Self-hosting `xaml-lint` against the plugin repo's own sample XAML.
- Corpus regression tester (SARIF baseline over curated open-source XAML).
- Auto-generated rule capability matrix.
- "Add a new rule" Claude skill (replaces a static contributor checklist).

## Definition of done for v1.0.0

All five §13 items closed, tag pushed, `AnalyzerReleases.Unshipped.md` graduated to a `## Release 1.0.0` header, `CHANGELOG.md` updated with a `[1.0.0]` section, plugin marketplace listing live, and the announcement post drafted. After tagging: drop the alpha suffix from `version.json` (see the `project_alpha_versioning` memory — stable semver reserved for v1+).
