# M5 — Pre-v1 polish — Design

> **Re-scope note (2026-04-19):** During execution the GitHub Pages migration was declined (user preferred serving rule docs directly from the `github.com/blob` rendering). The repo was transferred to the `XAML-Lint` GitHub organization. Tasks 3 (move `schema/v1/` under `docs/`), 4 (Jekyll scaffold + front matter), and 9 (enable Pages + smoke tests) were cancelled. Tasks 2, 5, 6, 7, 8 reduced to owner-only URL swaps (`jizc` → `XAML-Lint`) with URL shape preserved. The rest of the plan (Tasks 1, 10–16) executed as written.

| | |
|---|---|
| **Status** | Draft |
| **Date** | 2026-04-18 |
| **Author** | Jan Ivar Z. Carlsen |
| **Supersedes** | — |
| **Extends** | [2026-04-17-xaml-lint-design.md](2026-04-17-xaml-lint-design.md) |

## 1. Overview

M5 is a pre-release polish milestone that closes every item in the parent spec's §13 "Open items before v1 tag" list. It ships as `v0.5.0-alpha` and leaves `v1.0.0` as a pure graduation tag — no new work, just promote the alpha suffix away and cut the release.

**Motivation.** The §13 items — GitHub Pages migration, repo-location decision, `CONTRIBUTING.md`, NuGet package readme/icon, marketplace submission materials — are consequential enough to deserve a dedicated milestone. Folding them into the v1.0.0 tag moment would stack URL-cascading changes against release overhead; separating them gives stable URLs a bake-in window and keeps v1.0.0 low-risk.

**Non-goals for M5:**

- No new lint rules. Catalog is frozen at 20 IDs through v1.0.0.
- No behavior changes. Severities, dialects, suppression grammar, and config schema stay stable.
- No §12 work (LSP, auto-fix, corpus tester, self-hosting dogfood, "add a new rule" Claude skill).

## 2. Scope (the five §13 gates)

1. **Repo location decision.** Stay on `jizc/xaml-lint` or move to a new GitHub organization. Decision made and migration executed (if moving) before any URL work in phase B.
2. **GitHub Pages migration.** Move schema + rule docs off raw blob URLs to a Pages host. Cascades into every rule's `HelpUri`, the meta-test URL pattern, `$schema` URLs in sample configs, and README/CHANGELOG link rewrites.
3. **`CONTRIBUTING.md`.** Captures versioning policy (from parent spec §10) and an "add a new rule" checklist (`[XamlRule]` attribute, fixtures, `AnalyzerReleases.Unshipped.md`, `DocTool`, meta-tests).
4. **NuGet package readme + icon.** Icon sourced, `<PackageReadmeFile>` + `<PackageIcon>` wired into `XamlLint.Cli.csproj`, verified by inspecting the `dotnet pack` output.
5. **Plugin marketplace submission materials.** Listing copy, screenshots, long description committed under `docs/marketplace/` (or equivalent). Actual submission is either an M5-exit action or deferred to the v1.0.0 tag moment — the *materials* are the M5 deliverable, not the live listing.

## 3. Sequencing

Three phases with dependency ordering. Phase A gates phase B; phase C can partially overlap phase B but its final wiring (icon + readme reference) happens last.

### Phase A — Decide, then migrate location

Runs first because phase B's URL edits must target the final location. A mid-phase-B repo move would redo every URL edit.

1. Make the repo-location call. Default if user has no preference at decision time: stay on `jizc/`. Rationale: post-v1 moves are survivable (GitHub issues automatic redirects), whereas an unnecessary pre-v1 move burns time. Moving is preferred only if there's a concrete org already set up.
2. If moving: transfer repo, update `origin` remote locally, update `.claude-plugin/plugin.json` `homepage`, update CHANGELOG compare links, update any author strings pointing at the old location.

### Phase B — URL migration (mechanical, high-churn)

3. **Stand up GitHub Pages.** Jekyll default. The site serves `schema/v1/config.json`, `schema/v1/presets/*.json`, and `docs/rules/LX###.md`. Enable via repo Settings → Pages with source `main` branch `/docs` folder (or a dedicated `gh-pages` branch if front-matter collisions force it).
4. **Decide URL shape.** Recommendation: extensionless (`https://<owner>.github.io/xaml-lint/rules/LX100`), matching how tools like `dotnet format` host their rule docs. Alternative: `.md` suffix retained. Decision committed to this spec once made.
5. **Update `HelpUri` scheme.** Every `[XamlRule]` attribute in `src/XamlLint.Core/Rules/**`. Source-generated catalog + `DocTool` pick up the change automatically.
6. **Update meta-test URL pattern** (parent spec §8.2 already anticipated this migration; the regex just swaps base URLs).
7. **Regenerate schema + presets.** Run `DocTool` so `schema/v1/config.json` and `schema/v1/presets/*.json` carry the new `$schema` URL and any embedded links.
8. **Update `$schema` references** in the README config snippet and any committed example configs.
9. **Update README + CHANGELOG** — rewrite per-rule mentions and schema links to point at Pages where it improves reachability. Preserve the `docs/rules/LX###.md` repo links where the link is about source-of-truth content (the Pages host and the repo source are the same content; prefer Pages in user-facing copy, repo link in contributor-facing copy).
10. **CI gate.** `DocTool --check` must pass green. Any drift fails the check.

### Phase C — Release-surface polish (low-risk, partially parallelizable with B)

11. **`CONTRIBUTING.md`.** Sections: "Versioning policy" (verbatim from parent spec §10), "Adding a new rule" (walk through the canonical flow), "Running tests locally", "Doc tool and CI checks". Linked from README.
12. **NuGet package icon.** Source an icon — minimal vector mark is fine for v1 (an XAML angle bracket stylization, or a lint-crest). Committed under `assets/icon.png` at 128×128 PNG per NuGet spec.
13. **NuGet package readme + icon wiring.** Add to `XamlLint.Cli.csproj`:

    ```xml
    <PropertyGroup>
      <PackageReadmeFile>README.md</PackageReadmeFile>
      <PackageIcon>icon.png</PackageIcon>
    </PropertyGroup>
    <ItemGroup>
      <None Include="..\..\README.md" Pack="true" PackagePath="\" />
      <None Include="..\..\assets\icon.png" Pack="true" PackagePath="\" />
    </ItemGroup>
    ```

    Verify: `dotnet pack -c Release`, then inspect the nupkg with `unzip -l` or a NuGet viewer to confirm `README.md` and `icon.png` are at the root.
14. **Marketplace submission materials.** Commit under `docs/marketplace/`:
    - `listing.md` — short description, long description, feature bullets.
    - `screenshots/` — 2-3 PNGs: pretty formatter output in a terminal, Claude Code catching a diagnostic mid-edit, a SARIF result viewer if the marketplace wants CI-story framing.
    - Any marketplace metadata file the submission flow requires.

## 4. Decision points

Flagged so they're explicit and defaults are clear:

| # | Decision | Default | When needed |
|---|---|---|---|
| D1 | Repo location: stay on `jizc/` or move to an org | Stay on `jizc/` | Before phase B step 3 |
| D2 | GitHub Pages URL shape (extensionless vs `.md`) | Extensionless | Phase B step 4 |
| D3 | Icon source: commission, existing mark, or self-drawn | User input required when phase C starts | Phase C step 12 |
| D4 | Marketplace listing tone | Match existing README voice | Phase C step 14 |

## 5. Exit criteria

M5 graduates to `v0.5.0` when all true:

- [ ] Repo-location decision recorded; migration executed if applicable.
- [ ] Every `HelpUri` resolves on the Pages host (manual spot-check + meta-test pattern green).
- [ ] `xaml-lint.config.json`'s `$schema` URL resolves on the Pages host.
- [ ] `dotnet pack -c Release` produces a nupkg with embedded `README.md` and `icon.png` at the root; verified by inspection.
- [ ] `CONTRIBUTING.md` committed; linked from `README.md`.
- [ ] Marketplace submission materials committed under `docs/marketplace/`.
- [ ] `AnalyzerReleases.Unshipped.md` empty (no new rules in this milestone).
- [ ] CI green on all three OSes, all existing tests green, no regressions.
- [ ] `CHANGELOG.md` `[0.5.0]` entry authored following the established shape.
- [ ] Tag `v0.5.0` pushed.

## 6. Downstream doc updates

M5's existence forces edits in three existing documents. These edits are part of M5's deliverables.

### 6.1 Parent spec — `2026-04-17-xaml-lint-design.md`

- **§10 Milestones.** Insert an `### M5 — Pre-v1 polish (v0.5.0)` block between M4 and the `### v1.0.0 release` block. Mirror the prose shape of existing milestones.
- **§10 v1.0.0 release entry.** Collapse. Items M5 owns (Pages migration, `HelpUri` scheme change, repo-location call) move out. The v1.0.0 entry shrinks to: M5 complete, drop alpha suffix, tag push, announcement, marketplace listing go-live, final meta-test sweep.
- **§13 Open items.** Collapse to a one-liner stating §13 items are now tracked as M5 deliverables; keep the heading so existing links don't break.

### 6.2 Tracker — `docs/superpowers/v1-status.md`

- Add an M5 row to the Milestone progress table, status `🚧 In progress`.
- v1.0.0 row stays `🚧 Remaining`, but the Notes cell swaps "See §13 open items" for "Gates: M5 complete, alpha suffix dropped, announcement drafted."
- Replace the `§13 open items` section with an `M5 deliverables` section listing the same five items, framed as milestone deliverables with phase tags (A/B/C from §3 above).
- Refresh `Updated` date. Keep `Current release` at v0.4.0 until M5 ships.

### 6.3 `CHANGELOG.md`

- No change during M5 work. At tag time, add a `## [0.5.0] - YYYY-MM-DD` section following the established shape:
  - `### Added` — `CONTRIBUTING.md`, NuGet package readme, NuGet package icon, marketplace submission materials.
  - `### Changed` — GitHub Pages migration (`HelpUri` scheme, `$schema` URL, schema host).
  - Compare link and PR link at the bottom.

### 6.4 Not touched by M5

`AnalyzerReleases.Shipped.md`, `AnalyzerReleases.Unshipped.md`, any rule source file, any test source file. M5 is deliberately rules-inert. The only source code change is the `HelpUri` string on each `[XamlRule]` attribute, which is metadata.

## 7. Risks and mitigations

- **Risk:** repo-location change mid-work forces a second URL sweep. **Mitigation:** D1 gates phase B; no URL edits until D1 is resolved.
- **Risk:** GitHub Pages serves `.md` as raw markdown text instead of rendered HTML for some URL shapes. **Mitigation:** pick a Jekyll-compatible layout in phase B step 3; extensionless URLs make this explicit.
- **Risk:** NuGet package-icon PNG exceeds 1MB limit or wrong dimensions. **Mitigation:** spec requires 128×128 PNG at phase C step 12; verified on `dotnet pack` before committing.
- **Risk:** marketplace submission flow requires assets not yet known (specific image dimensions, license checkboxes). **Mitigation:** M5 exit only requires *materials committed*, not live listing. Submission itself can slide to v1.0.0 moment.
