# M5 — Pre-v1 Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. One commit per task, each standalone. After each task touching code: `dotnet build xaml-lint.slnx --configuration Release` clean and `dotnet test --solution xaml-lint.slnx` green. Run `dotnet run --project src/XamlLint.DocTool --configuration Release -- --check` after any task that moves schema/preset outputs or edits `HelpUri` to confirm no doc drift.

**Goal:** Close every item in the parent spec §13 "Open items before v1 tag" list in a single dedicated milestone, tag **v0.5.0** (NuGet publishes as `0.5.0-alpha` per the repo's 0.x alpha-versioning convention), and leave `v1.0.0` as a pure graduation tag. Deliverables: GitHub Pages migration for schema + rule docs, repo-location decision (migration if moving), `CONTRIBUTING.md`, NuGet package readme + icon, and marketplace submission materials under `docs/marketplace/`.

**Architecture:** M5 is mechanical and rules-inert. The only `.cs` edits are (a) `HelpUri` string updates in 26 call sites, (b) output-path changes in `SchemaWriter` + `PresetWriter`, (c) the meta-test URL regex. All rule behavior, severities, dialect masks, suppression grammar, and config schema shape stay stable. The bulk of the work is under `docs/` (Pages scaffold, rule-doc front matter, `CONTRIBUTING.md`, marketplace materials), plus a moved `schema/v1/` tree that now sits under `docs/` so GitHub Pages can serve it from the `/docs` source folder.

**Tech Stack:** .NET 10, existing `XamlLint.DocTool` post-build/CI verifier, Jekyll (GitHub Pages default — no custom theme; barebones `_config.yml` for rendering), `Nerdbank.GitVersioning` for the `0.4-alpha` → `0.5-alpha` bump and the eventual `v0.5.0` tag. No new NuGet packages. No new rules. No test-rule code changes beyond the single meta-test regex.

---

## Notes before starting

**Phase ordering (from spec §3).** Phase A (D1 decision + optional migration) gates phase B (URL migration). Phase B's URL edits must target the final repo location; a mid-phase-B repo move would redo every URL edit. Phase C (release-surface polish) can partially parallelize with B, but this plan serializes it for simplicity — CONTRIBUTING.md / icon / csproj wiring / marketplace materials all live after B settles.

**D1 — repo location.** Default baked into this plan: **stay on `jizc/xaml-lint`**, per spec §4 D1 default. Rationale: post-v1 moves are survivable (GitHub issues automatic redirects), whereas an unnecessary pre-v1 move burns time. If the user opts to move to a new organization before executing this plan, re-run Task 2 with the new owner name and substitute `<new-owner>` for `jizc` everywhere this plan hardcodes the owner. Every downstream Pages URL in Tasks 3–9, every preset/schema `$id` / `$schema`, and every HelpUri becomes `https://<new-owner>.github.io/xaml-lint/...` instead.

**D2 — URL shape.** Decided: **extensionless for rule docs** (matches `dotnet format`'s rule doc hosting). Rule docs are served by Jekyll at `https://jizc.github.io/xaml-lint/rules/LX###` with GitHub Pages' built-in "try `.html` fallback" handling the no-extension request. Schema + presets keep their `.json` extension because they're data files consumed by editors and the CLI: `https://jizc.github.io/xaml-lint/schema/v1/config.json`, `.../schema/v1/presets/xaml-lint-{off,recommended,strict}.json`.

**D3 — icon.** Deferred to Task 11. Plan provides a concrete PowerShell+GDI fallback that produces a usable 128×128 PNG if the user does not supply one.

**D4 — marketplace listing tone.** Match existing README voice (terse, technical, no marketing puffery).

**Where HelpUri lives.** The project has **26 HelpUri string call sites** that must flip together, proven by `rg HelpUri -g '!**/docs/**' -g '!**/plans/**'`:

- **20 rule-attribute sites** — each `[XamlRule(HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX###.md")]` under `src/XamlLint.Core/Rules/**/*.cs`.
- **3 const sites**:
  - `src/XamlLint.Core/RuleDispatcher.cs:13` — `public const string HelpUriLX006`
  - `src/XamlLint.Core/Suppressions/PragmaParser.cs:15` — `private const string HelpUriLX002`
  - `src/XamlLint.Cli/Config/ConfigLoader.cs:17` — `private const string HelpUriLX003`
- **3 inline sites** in `src/XamlLint.Cli/LintPipeline.cs` for LX005, LX004, LX001 (see the grep output in spec-prep; lines ~70, ~85, ~105).

The meta-test `Every_rule_has_help_uri_matching_pattern` in `tests/XamlLint.Core.Tests/Meta/CatalogMetaTest.cs:25-30` asserts the 20 `[XamlRule]` attribute URLs against a single regex; the 3 consts and 3 inline URLs are caught only by downstream integration tests (none today exercise the tool-diagnostic HelpUri strings directly).

**Where `schema/v1/` lives.** The project has **3 code references** to the path:

- `src/XamlLint.DocTool/SchemaWriter.cs:12` — output path.
- `src/XamlLint.DocTool/PresetWriter.cs:12` — output dir.
- `src/XamlLint.Cli/XamlLint.Cli.csproj:42` — `<EmbeddedResource Include="..\..\schema\v1\presets\*.json">`.

Plus 4 on-disk JSON files (`schema/v1/config.json`, `schema/v1/presets/xaml-lint-{off,recommended,strict}.json`) that get regenerated by DocTool. The move to `docs/schema/v1/` updates exactly these references; the LogicalName (`XamlLint.Cli.Presets.%(Filename).json`) stays identical so nothing that reads the embedded resource by name has to change.

**Where `$schema` / `$id` strings live.** Three code sites write the URL:

- `src/XamlLint.DocTool/SchemaWriter.cs:33` — `$id` in generated `config.json`.
- `src/XamlLint.DocTool/PresetWriter.cs:40` — `$schema` in each generated preset.
- Two user-facing doc sites hardcode the URL: `README.md:41` (config snippet) and `docs/config-reference.md:9` (example). These are authored by humans, not generated.

**Meta-test regex.** Currently `^https://github\.com/jizc/xaml-lint/blob/main/docs/rules/LX\d{3}\.md$`. Flips to `^https://jizc\.github\.io/xaml-lint/rules/LX\d{3}$` in Task 5 (the "red" step before Task 6's URL updates land).

**Rule-doc front matter.** GitHub Pages renders Markdown via Jekyll, but only for files with YAML front matter. The 20 rule-doc files plus 7 category-overview files in `docs/rules/` all currently start with `#`, not `---`. Task 4 adds a minimal `---\n---\n` fence to every one and updates `DocStubWriter.BuildStub` so new stubs emit the fence automatically. This does not count as a "rule source" or "test source" change under spec §6.4 — it's doc plumbing.

**AnalyzerReleases invariant.** `AnalyzerReleases.Unshipped.md` already has an empty new-rules table (only headers). M5 touches no rules, so no rows get added, and the graduation step at tag time appends nothing to `Shipped.md`. Task 15 is a sanity check, not a graduation.

**NuGet package versioning at tag time.** `version.json` base is `0.4-alpha`; Task 1 bumps it to `0.5-alpha`. Nerdbank.GitVersioning produces nupkgs as `0.5.N-alpha` on main and `0.5.0-alpha` on the `v0.5.0` tag (matching the repo's "every 0.x tag ships as `x.y.N-alpha`" convention — see `project_alpha_versioning` memory). The git tag itself is `v0.5.0` (no `-alpha` in the tag).

**Test runner invocation.** `dotnet test --solution xaml-lint.slnx` (MTP requires the `--solution` flag; `dotnet test` with a positional path is ignored). For a single meta-test: `dotnet test --solution xaml-lint.slnx --filter-method "*Every_rule_has_help_uri*"`.

**Existing files that M5 must not destroy.** Every rule body under `src/XamlLint.Core/Rules/**/*.cs` (only the `HelpUri` attribute string changes — never the `Analyze()` method, never other metadata fields); every rule-doc `# LX###:` heading and body in `docs/rules/*.md` (only front-matter fence is added); every `AnalyzerReleases.Shipped.md` section (M5 adds no rules); `version.json`'s `publicReleaseRefSpec` and `release` blocks; the `XamlLint.Core.SourceGen` project source; every test except the one meta-test regex.

---

## File structure

**New files created in M5:**

```
CONTRIBUTING.md                                   # Task 10
assets/
  icon.png                                        # Task 11 (128×128 PNG)
docs/
  _config.yml                                     # Task 4 (Jekyll config)
  schema/                                         # Task 3 (moved from schema/)
    v1/
      config.json                                 # regenerated by DocTool in Task 7
      presets/
        xaml-lint-off.json                        # regenerated by DocTool in Task 7
        xaml-lint-recommended.json                # regenerated by DocTool in Task 7
        xaml-lint-strict.json                     # regenerated by DocTool in Task 7
  marketplace/                                    # Task 13
    listing.md
    screenshots/
      README.md                                   # describes expected screenshot contents
```

**Files deleted in M5:**

```
schema/                                           # old location; removed in Task 3
```

**Files modified in M5:**

- `version.json` — Task 1 (`0.4-alpha` → `0.5-alpha`).
- `src/XamlLint.Cli/XamlLint.Cli.csproj` — Task 3 (EmbeddedResource path) + Task 12 (PackageIcon wiring).
- `src/XamlLint.DocTool/SchemaWriter.cs` — Task 3 (output path) + Task 7 (`$id` URL).
- `src/XamlLint.DocTool/PresetWriter.cs` — Task 3 (output dir) + Task 7 (`$schema` URL).
- `src/XamlLint.DocTool/DocStubWriter.cs` — Task 4 (emit front matter in stubs).
- `docs/rules/*.md` — Task 4 (front matter on 27 files: LX001.md, LX002.md, LX003.md, LX004.md, LX005.md, LX006.md, LX100.md, LX101.md, LX102.md, LX103.md, LX104.md, LX200.md, LX201.md, LX300.md, LX301.md, LX400.md, LX500.md, LX501.md, LX502.md, LX600.md, tool.md, layout.md, bindings.md, naming.md, resources.md, input.md, deprecated.md).
- `tests/XamlLint.Core.Tests/Meta/CatalogMetaTest.cs` — Task 5 (HelpUri regex).
- `src/XamlLint.Core/Rules/**/LX*.cs` — Task 6 (HelpUri string, 20 files).
- `src/XamlLint.Core/RuleDispatcher.cs` — Task 6 (`HelpUriLX006`).
- `src/XamlLint.Core/Suppressions/PragmaParser.cs` — Task 6 (`HelpUriLX002`).
- `src/XamlLint.Cli/Config/ConfigLoader.cs` — Task 6 (`HelpUriLX003`).
- `src/XamlLint.Cli/LintPipeline.cs` — Task 6 (3 inline URLs).
- `README.md` — Task 8 (`$schema` URL) + Task 10 (CONTRIBUTING.md link).
- `docs/config-reference.md` — Task 8 (`$schema` URL).
- `docs/superpowers/specs/2026-04-17-xaml-lint-design.md` — Task 14 (§10 M5 insert, §10 v1.0.0 collapse, §13 collapse).
- `docs/superpowers/v1-status.md` — Task 14 (M5 row, M5 deliverables section, date).
- `.claude-plugin/plugin.json` — Task 15 (version bump to `0.5.0`).
- `CHANGELOG.md` — Task 15 (`[0.5.0]` section + compare link).

---

## Task 1: Create branch and bump version

Starts M5 work on its own branch and bumps Nerdbank.GitVersioning's base version so every development build on this branch produces `0.5.0-alpha.N` and the eventual `v0.5.0` tag graduates per the repo's alpha-versioning convention.

**Files:**
- Modify: `version.json`

- [ ] **Step 1: Create and check out the branch**

```bash
git -C D:/GitHub/jizc/xaml-lint checkout -b m5-pre-v1-polish
```

Expected: `Switched to a new branch 'm5-pre-v1-polish'`.

- [ ] **Step 2: Bump version.json**

Edit `version.json`, change `"version": "0.4-alpha"` to `"version": "0.5-alpha"`. Nothing else in the file changes.

Resulting file:

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/main/src/NerdBank.GitVersioning/version.schema.json",
  "version": "0.5-alpha",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+"
  ],
  "nugetPackageVersion": {
    "semVer": 2
  },
  "release": {
    "branchName": "release/v{version}",
    "versionIncrement": "minor",
    "firstUnstableTag": "alpha"
  }
}
```

- [ ] **Step 3: Verify build and tests still green**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: build succeeds; all existing tests pass. Confirms the version bump didn't break anything mechanical.

- [ ] **Step 4: Commit**

```bash
git add version.json
git commit -m "chore(version): bump Nerdbank base version to 0.5-alpha for M5"
```

---

## Task 2: Phase A — repo-location decision

Acts on the D1 decision before any URL edits. Default under this plan: **stay on `jizc/`**. If the user opts to move to an organization before running this task, substitute `<new-owner>` for `jizc` in every URL shown in Tasks 3–9 and also update `.claude-plugin/plugin.json` `homepage`, CHANGELOG compare links, and `LICENSE` author block. This task records the decision and performs the optional migration only when needed.

**Files:**
- Modify (only if moving): `.claude-plugin/plugin.json`, `CHANGELOG.md` (compare links), `CLAUDE.md` (if it mentions the owner explicitly — it doesn't today).

- [ ] **Step 1: Confirm D1 outcome**

Default: stay on `jizc/xaml-lint`. If the user has set up an organization and wants to move, capture the new owner name now.

- [ ] **Step 2a (stay path): no file edits**

Proceed to Task 3 with owner = `jizc`. No commit for Task 2.

- [ ] **Step 2b (move path, only if moving): transfer and update references**

```bash
# GitHub web UI: repo Settings → Transfer ownership → <new-owner>/xaml-lint
# Then locally:
git remote set-url origin git@github.com:<new-owner>/xaml-lint.git
git remote -v  # verify
```

Edit `.claude-plugin/plugin.json` `homepage` from `https://github.com/jizc/xaml-lint` to `https://github.com/<new-owner>/xaml-lint`.

Edit every compare link in `CHANGELOG.md` (lines 83-91):

```
[Unreleased]: https://github.com/<new-owner>/xaml-lint/compare/v0.4.0...HEAD
[0.4.0]: https://github.com/<new-owner>/xaml-lint/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/<new-owner>/xaml-lint/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/<new-owner>/xaml-lint/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/<new-owner>/xaml-lint/releases/tag/v0.1.0
[#2]: https://github.com/<new-owner>/xaml-lint/pull/2
[#3]: https://github.com/<new-owner>/xaml-lint/pull/3
[#4]: https://github.com/<new-owner>/xaml-lint/pull/4
[#5]: https://github.com/<new-owner>/xaml-lint/pull/5
```

- [ ] **Step 3 (move path only): verify and commit**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green.

```bash
git add .claude-plugin/plugin.json CHANGELOG.md
git commit -m "chore(repo): migrate owner references to <new-owner>"
```

If staying on `jizc/`, skip Steps 2b and 3 and proceed directly to Task 3.

---

## Task 3: Move schema tree under docs/ and update code references

GitHub Pages source is the `main` branch `/docs` folder (per spec §3 step 3). For Pages to serve schema + presets at `https://<owner>.github.io/xaml-lint/schema/v1/config.json`, the JSON files must live under `docs/schema/v1/`. This task moves them and updates the three code references; the CLI's embedded-resource `LogicalName` stays unchanged, so runtime preset loading is unaffected.

**Files:**
- Move: `schema/v1/` → `docs/schema/v1/`
- Delete: `schema/` (entire tree)
- Modify: `src/XamlLint.DocTool/SchemaWriter.cs`, `src/XamlLint.DocTool/PresetWriter.cs`, `src/XamlLint.Cli/XamlLint.Cli.csproj`

- [ ] **Step 1: Move the schema tree with git mv**

```bash
mkdir -p docs/schema/v1/presets
git mv schema/v1/config.json docs/schema/v1/config.json
git mv schema/v1/presets/xaml-lint-off.json docs/schema/v1/presets/xaml-lint-off.json
git mv schema/v1/presets/xaml-lint-recommended.json docs/schema/v1/presets/xaml-lint-recommended.json
git mv schema/v1/presets/xaml-lint-strict.json docs/schema/v1/presets/xaml-lint-strict.json
rmdir schema/v1/presets schema/v1 schema
git status  # verify: 4 renames + empty schema/ removed
```

Expected: four `renamed:` entries, no untracked files, `schema/` no longer exists.

- [ ] **Step 2: Update SchemaWriter output path**

In `src/XamlLint.DocTool/SchemaWriter.cs`, line 12:

```csharp
// Before:
var target = Path.Combine(repoRoot, "schema", "v1", "config.json");

// After:
var target = Path.Combine(repoRoot, "docs", "schema", "v1", "config.json");
```

(Do not touch the `$id` URL string on line 33 yet — that's Task 7.)

- [ ] **Step 3: Update PresetWriter output dir**

In `src/XamlLint.DocTool/PresetWriter.cs`, line 12:

```csharp
// Before:
var dir = Path.Combine(repoRoot, "schema", "v1", "presets");

// After:
var dir = Path.Combine(repoRoot, "docs", "schema", "v1", "presets");
```

(Do not touch the `$schema` URL on line 40 yet — that's Task 7.)

- [ ] **Step 4: Update CLI embedded-resource include path**

In `src/XamlLint.Cli/XamlLint.Cli.csproj`, lines 42-44:

```xml
<!-- Before: -->
<ItemGroup>
  <EmbeddedResource Include="..\..\schema\v1\presets\*.json">
    <LogicalName>XamlLint.Cli.Presets.%(Filename).json</LogicalName>
  </EmbeddedResource>
</ItemGroup>

<!-- After: -->
<ItemGroup>
  <EmbeddedResource Include="..\..\docs\schema\v1\presets\*.json">
    <LogicalName>XamlLint.Cli.Presets.%(Filename).json</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

LogicalName is unchanged — callers still resolve embedded presets via `XamlLint.Cli.Presets.xaml-lint-recommended.json` etc.

- [ ] **Step 5: Build and run tests**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green. The move is path-only; presets load by LogicalName, so tests that use embedded presets (e.g., `extends: xaml-lint:recommended`) continue to find them.

- [ ] **Step 6: Run DocTool --check**

```bash
dotnet run --project src/XamlLint.DocTool --configuration Release --no-build -- --check
```

Expected: "DocTool --check: no drift." The files have been moved verbatim; DocTool regenerates to the same content at the new path.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "refactor: move schema/v1 under docs/ so GitHub Pages can serve it"
```

---

## Task 4: Add Jekyll scaffold and rule-doc front matter

GitHub Pages renders Markdown as HTML only for files with YAML front matter. This task stands up a minimal Jekyll config at `docs/_config.yml` and adds an empty `---\n---\n` fence to every `.md` file under `docs/rules/`. It also updates `DocStubWriter` so new rule-doc stubs emit the fence automatically. Schema + preset JSON files need no front matter — Jekyll copies non-Jekyll files as-is.

**Files:**
- Create: `docs/_config.yml`
- Modify: every `.md` file in `docs/rules/` (27 files), `src/XamlLint.DocTool/DocStubWriter.cs`

- [ ] **Step 1: Create docs/_config.yml**

```yaml
title: xaml-lint
description: A Claude Code plugin that lints XAML files for common issues.
markdown: kramdown
exclude:
  - superpowers
```

The `exclude: [superpowers]` keeps specs/plans/status pages out of the Pages build — those are for contributors and live in the repo tree, not on the site. Schema JSON files have no front matter and are copied as-is (Jekyll default for files without a `---` fence).

- [ ] **Step 2: Add empty front matter to every rule doc**

For each of the 27 files below, prepend `---\n---\n\n` (an empty front matter fence followed by a blank line) before the existing `# LXnnn:` or `# <Category>` heading:

Rule docs (20): `docs/rules/LX001.md`, `LX002.md`, `LX003.md`, `LX004.md`, `LX005.md`, `LX006.md`, `LX100.md`, `LX101.md`, `LX102.md`, `LX103.md`, `LX104.md`, `LX200.md`, `LX201.md`, `LX300.md`, `LX301.md`, `LX400.md`, `LX500.md`, `LX501.md`, `LX502.md`, `LX600.md`.

Category overviews (7): `docs/rules/tool.md`, `layout.md`, `bindings.md`, `naming.md`, `resources.md`, `input.md`, `deprecated.md`.

Example transformation (`docs/rules/LX100.md`):

```markdown
---
---

# LX100: Grid.Row without matching RowDefinition

<!-- generated stub; edit freely. Upstream: RXT101. -->

## Cause
...
```

Use an Edit tool per file, or a single scripted pass. Verify no file was missed: `rg -l '^---' docs/rules/*.md | wc -l` must be **27**.

- [ ] **Step 3: Update DocStubWriter to emit front matter in new stubs**

In `src/XamlLint.DocTool/DocStubWriter.cs`, lines 51-73, prepend the fence to the `BuildStub` template:

```csharp
private static string BuildStub(RuleMetadata m) => $$"""
---
---

# {{m.Id}}: {{m.Title}}

{{StubSentinel}} Upstream: {{m.UpstreamId ?? "—"}}. -->

## Cause

TBD — describe what triggers this rule.

## Rule description

TBD — longer-form explanation with inline-annotated XAML examples.

## How to fix violations

TBD — concrete steps the reader takes.

## How to suppress violations

- `<!-- xaml-lint disable once {{m.Id}} -->` (inline, one element)
- `<!-- xaml-lint disable {{m.Id}} -->` … `<!-- xaml-lint restore {{m.Id}} -->` (block)
- `xaml-lint.config.json` → `"rules": { "{{m.Id}}": "off" }` (file/project)
""";
```

- [ ] **Step 4: Run build and full test suite**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green. Only doc files and a stub template string changed; no rule or test logic is affected.

- [ ] **Step 5: Run DocTool --check**

```bash
dotnet run --project src/XamlLint.DocTool --configuration Release --no-build -- --check
```

Expected: "DocTool --check: no drift." The 20 existing rule docs already have front matter (added in Step 2), and the stub template change doesn't affect existing files.

- [ ] **Step 6: Commit**

```bash
git add docs/_config.yml docs/rules/*.md src/XamlLint.DocTool/DocStubWriter.cs
git commit -m "docs(pages): add Jekyll scaffold and front matter for rule docs"
```

---

## Task 5: Meta-test red — update HelpUri regex

TDD entry point for the URL migration. Updating the meta-test regex first causes the 20 `[XamlRule]`-attribute call sites to fail the meta-test until Task 6 flips them. The 3 const sites and 3 inline sites in the CLI are not asserted by meta-tests, but Task 6 flips them in the same pass for consistency.

**Files:**
- Modify: `tests/XamlLint.Core.Tests/Meta/CatalogMetaTest.cs`

- [ ] **Step 1: Update the regex**

In `tests/XamlLint.Core.Tests/Meta/CatalogMetaTest.cs`, line 27:

```csharp
// Before:
var pattern = new Regex(@"^https://github\.com/jizc/xaml-lint/blob/main/docs/rules/LX\d{3}\.md$");

// After:
var pattern = new Regex(@"^https://jizc\.github\.io/xaml-lint/rules/LX\d{3}$");
```

(If D1 chose a different owner, substitute `<new-owner>` for `jizc`.)

- [ ] **Step 2: Run the meta-test to confirm red**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --filter-method "*Every_rule_has_help_uri*"
```

Expected: **20 failures**, one per rule, each with a message like `"LX100 HelpUri must match expected pattern; got 'https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX100.md'"`. No other tests should fail.

- [ ] **Step 3: Commit the red**

```bash
git add tests/XamlLint.Core.Tests/Meta/CatalogMetaTest.cs
git commit -m "test(meta): point HelpUri regex at Pages URL (expected red pre-migration)"
```

This intentional-red commit documents the migration intent. The next task turns it green.

---

## Task 6: Meta-test green — update every HelpUri call site

Flips all 26 HelpUri strings in one pass. Attribute strings satisfy the meta-test from Task 5; constants and inline strings stay consistent with the new URL shape so tool diagnostics (LX001–LX006) point at the same Pages host.

**Files:**
- Modify: 20 rule files under `src/XamlLint.Core/Rules/**/*.cs`
- Modify: `src/XamlLint.Core/RuleDispatcher.cs`
- Modify: `src/XamlLint.Core/Suppressions/PragmaParser.cs`
- Modify: `src/XamlLint.Cli/Config/ConfigLoader.cs`
- Modify: `src/XamlLint.Cli/LintPipeline.cs`

- [ ] **Step 1: Update rule-attribute HelpUri strings**

For each of the 20 rule files, replace the URL pattern. The transformation is mechanical: `https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX###.md` → `https://jizc.github.io/xaml-lint/rules/LX###`.

Files (in ID order — each has exactly one HelpUri attribute string to change):

- `src/XamlLint.Core/Rules/Tool/LX001_MalformedXaml.cs:9`
- `src/XamlLint.Core/Rules/Tool/LX002_UnrecognizedPragma.cs:8`
- `src/XamlLint.Core/Rules/Tool/LX003_MalformedConfig.cs:8`
- `src/XamlLint.Core/Rules/Tool/LX004_CannotReadFile.cs:8`
- `src/XamlLint.Core/Rules/Tool/LX005_SkippingNonXaml.cs:8`
- `src/XamlLint.Core/Rules/Tool/LX006_InternalErrorInRule.cs:8`
- `src/XamlLint.Core/Rules/Layout/LX100_GridRowWithoutDefinition.cs:12`
- `src/XamlLint.Core/Rules/Layout/LX101_GridColumnWithoutDefinition.cs:12`
- `src/XamlLint.Core/Rules/Layout/LX102_GridRowSpanExceedsRows.cs:12`
- `src/XamlLint.Core/Rules/Layout/LX103_GridColumnSpanExceedsColumns.cs:12`
- `src/XamlLint.Core/Rules/Layout/LX104_GridDefinitionShorthandUnsupported.cs:11`
- `src/XamlLint.Core/Rules/Bindings/LX200_SelectedItemTwoWay.cs:11`
- `src/XamlLint.Core/Rules/Bindings/LX201_PreferXBind.cs:11`
- `src/XamlLint.Core/Rules/Naming/LX300_XNameCasing.cs:11`
- `src/XamlLint.Core/Rules/Naming/LX301_XUidCasing.cs:11`
- `src/XamlLint.Core/Rules/Resources/LX400_HardcodedString.cs:11`
- `src/XamlLint.Core/Rules/Input/LX500_TextBoxWithoutInputScope.cs:11`
- `src/XamlLint.Core/Rules/Input/LX501_SliderMinimumGreaterThanMaximum.cs:12`
- `src/XamlLint.Core/Rules/Input/LX502_StepperMinimumGreaterThanMaximum.cs:12`
- `src/XamlLint.Core/Rules/Deprecated/LX600_MediaElementDeprecated.cs:11`

Per-file shape — example for LX100:

```csharp
// Before:
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX100.md")]

// After:
    HelpUri = "https://jizc.github.io/xaml-lint/rules/LX100")]
```

- [ ] **Step 2: Update the 3 const sites**

`src/XamlLint.Core/RuleDispatcher.cs:13`:

```csharp
// Before:
public const string HelpUriLX006 = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX006.md";

// After:
public const string HelpUriLX006 = "https://jizc.github.io/xaml-lint/rules/LX006";
```

`src/XamlLint.Core/Suppressions/PragmaParser.cs:15`:

```csharp
// Before:
private const string HelpUriLX002 = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX002.md";

// After:
private const string HelpUriLX002 = "https://jizc.github.io/xaml-lint/rules/LX002";
```

`src/XamlLint.Cli/Config/ConfigLoader.cs:17`:

```csharp
// Before:
private const string HelpUriLX003 = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX003.md";

// After:
private const string HelpUriLX003 = "https://jizc.github.io/xaml-lint/rules/LX003";
```

- [ ] **Step 3: Update the 3 inline sites in LintPipeline**

In `src/XamlLint.Cli/LintPipeline.cs`, update the three `https://github.com/jizc/xaml-lint/blob/main/docs/rules/LXnnn.md` literal strings passed to diagnostic constructors (around lines 70 / 85 / 105) so they read `https://jizc.github.io/xaml-lint/rules/LXnnn` (for LX005, LX004, LX001 respectively). The before/after shape of each line:

```csharp
// Before:
                        "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX005.md"));

// After:
                        "https://jizc.github.io/xaml-lint/rules/LX005"));
```

Apply the same transform to the LX004 and LX001 lines.

- [ ] **Step 4: Sanity check — no stale blob URLs remain in source or tests**

```bash
rg 'github\.com/jizc/xaml-lint/blob' src/ tests/
```

Expected: **zero matches**. If anything remains, it is a miss in Steps 1–3 — fix before moving on.

- [ ] **Step 5: Run the meta-test to confirm green**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --filter-method "*Every_rule_has_help_uri*"
```

Expected: 1 passing test.

- [ ] **Step 6: Run the full test suite**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --verbosity normal
```

Expected: all tests green. Any CLI integration test that pins an exact HelpUri would need its expected value updated — none exist today, but confirm via the test run.

- [ ] **Step 7: Commit**

```bash
git add src/ tests/
git commit -m "refactor: point HelpUri at GitHub Pages URL across all call sites"
```

---

## Task 7: Update DocTool URL constants and regenerate schema + presets

Flips the `$id` URL in generated `config.json` and the `$schema` URL in generated presets from `raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json` to `jizc.github.io/xaml-lint/schema/v1/config.json`. Regenerates the on-disk files via DocTool.

**Files:**
- Modify: `src/XamlLint.DocTool/SchemaWriter.cs`, `src/XamlLint.DocTool/PresetWriter.cs`
- Modify (via regeneration): `docs/schema/v1/config.json`, `docs/schema/v1/presets/*.json`

- [ ] **Step 1: Update SchemaWriter `$id`**

In `src/XamlLint.DocTool/SchemaWriter.cs:33`:

```csharp
// Before:
w.WriteString("$id", "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json");

// After:
w.WriteString("$id", "https://jizc.github.io/xaml-lint/schema/v1/config.json");
```

- [ ] **Step 2: Update PresetWriter `$schema`**

In `src/XamlLint.DocTool/PresetWriter.cs:40`:

```csharp
// Before:
w.WriteString("$schema", "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json");

// After:
w.WriteString("$schema", "https://jizc.github.io/xaml-lint/schema/v1/config.json");
```

- [ ] **Step 3: Build**

```bash
dotnet build xaml-lint.slnx --configuration Release
```

Expected: green.

- [ ] **Step 4: Regenerate schema and presets (not --check)**

```bash
dotnet run --project src/XamlLint.DocTool --configuration Release --no-build
```

Expected stdout:

```
Updated schema: …/docs/schema/v1/config.json
Updated preset: …/docs/schema/v1/presets/xaml-lint-off.json
Updated preset: …/docs/schema/v1/presets/xaml-lint-recommended.json
Updated preset: …/docs/schema/v1/presets/xaml-lint-strict.json
```

Verify the regenerated files reference the new URL:

```bash
rg '\$schema|\$id' docs/schema/v1/
```

Expected: `$id` in `config.json` and `$schema` in the three presets all read `https://jizc.github.io/xaml-lint/schema/v1/config.json`. `config.json` also has the `https://json-schema.org/draft/2020-12/schema` self-reference — that is the JSON Schema meta-schema URL, leave untouched.

- [ ] **Step 5: Run DocTool --check**

```bash
dotnet run --project src/XamlLint.DocTool --configuration Release --no-build -- --check
```

Expected: "DocTool --check: no drift."

- [ ] **Step 6: Run full test suite**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green. Tests that load embedded presets (e.g., `extends: xaml-lint:recommended` in CLI integration) see the new `$schema` but that is not asserted equal to any expected value — it's just passthrough metadata.

- [ ] **Step 7: Commit**

```bash
git add src/XamlLint.DocTool/ docs/schema/
git commit -m "refactor(doctool): point schema and preset \$schema/\$id at Pages URL"
```

---

## Task 8: Update `$schema` in user-facing docs

Two hand-authored doc files hardcode the old `raw.githubusercontent.com` URL. Flip both to the Pages URL so users copy-pasting the config snippet get the new host.

**Files:**
- Modify: `README.md`
- Modify: `docs/config-reference.md`

- [ ] **Step 1: Update README.md**

In `README.md:41`:

```json
// Before:
  "$schema": "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json",

// After:
  "$schema": "https://jizc.github.io/xaml-lint/schema/v1/config.json",
```

- [ ] **Step 2: Update docs/config-reference.md**

In `docs/config-reference.md:9`:

```json
// Before:
  "$schema": "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json",

// After:
  "$schema": "https://jizc.github.io/xaml-lint/schema/v1/config.json",
```

- [ ] **Step 3: Sanity sweep for stale URLs in user-facing docs**

```bash
rg 'raw\.githubusercontent\.com/jizc/xaml-lint' README.md docs/
```

Expected: **zero matches**. The `docs/superpowers/` tree (plans and specs) is historical and may still mention the old URL — those are contributor-facing docs and deliberately preserve the pre-migration URL for context. Do **not** rewrite them.

```bash
rg 'github\.com/jizc/xaml-lint/blob/main/docs/rules' README.md docs/ -g '!**/superpowers/**'
```

Expected: **zero matches**.

- [ ] **Step 4: Commit**

```bash
git add README.md docs/config-reference.md
git commit -m "docs: point \$schema references at GitHub Pages URL"
```

---

## Task 9: Enable GitHub Pages and verify URLs resolve

The final Phase B step activates Pages in repo settings and smoke-tests that every migrated URL resolves. Pages activation is a manual web-UI step; this task records the procedure and the verification commands.

**Files:** no code changes. Documentation action + manual verification.

- [ ] **Step 1: Enable Pages via repo Settings**

In GitHub web UI (`https://github.com/jizc/xaml-lint/settings/pages`):

- Source: **Deploy from a branch**
- Branch: `main`
- Folder: **`/docs`**
- Click Save.

First build takes 1–5 minutes. The Pages site goes live at `https://jizc.github.io/xaml-lint/`.

- [ ] **Step 2: Wait for initial build to finish**

GitHub reports build status in Settings → Pages and in Actions tab under "pages build and deployment". Wait for the green checkmark before proceeding.

- [ ] **Step 3: Smoke-test the migrated URLs**

```bash
# Rule docs (extensionless) — spot-check 3
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/rules/LX001
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/rules/LX100
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/rules/LX600

# Schema
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/schema/v1/config.json

# Presets (all 3)
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/schema/v1/presets/xaml-lint-off.json
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/schema/v1/presets/xaml-lint-recommended.json
curl -sSfL -o /dev/null -w "%{http_code} %{url_effective}\n" https://jizc.github.io/xaml-lint/schema/v1/presets/xaml-lint-strict.json
```

Expected: each command prints `200 <url>` with a 200 status. Any 404 means the Pages build didn't see that file — most commonly because front matter is missing (Task 4 Step 2) or the file is under an excluded path. Fix and push before declaring Phase B done.

- [ ] **Step 4: Verify rendered output**

Open `https://jizc.github.io/xaml-lint/rules/LX100` in a browser. Expected: the `LX100.md` content renders as HTML (heading, cause, examples). If it renders as raw Markdown text, front matter is missing from that file or `_config.yml` is malformed.

Open `https://jizc.github.io/xaml-lint/schema/v1/config.json`. Expected: the JSON is returned verbatim (the `$id` field reads the new Pages URL; no HTML wrapping).

- [ ] **Step 5: No commit necessary**

This task is activation + verification. No files changed. Proceed to Phase C.

---

## Task 10: Write CONTRIBUTING.md and link from README

The first Phase C deliverable. Captures the versioning policy verbatim from parent spec §10 and the "add a new rule" flow; links from README.

**Files:**
- Create: `CONTRIBUTING.md`
- Modify: `README.md`

- [ ] **Step 1: Create CONTRIBUTING.md**

```markdown
# Contributing to xaml-lint

Thanks for considering a contribution. This project is a Claude Code plugin that lints XAML files; the primary consumer is Claude itself, but contributions that make it better for human and CI use are equally welcome.

## Versioning policy

`xaml-lint` follows [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html). For rule-catalog changes specifically:

- **Rule additions** → minor version bump.
- **Rule removals** → major version bump. Deprecated rules stay in the catalog with `Deprecated = true` and (usually) `ReplacedBy = "LX###"` pointing at a successor; they are not deleted.
- **Severity downgrades** (e.g., `warning` → `info`) → minor version bump.
- **Severity upgrades** (e.g., `warning` → `error`) → major version bump.

0.x releases ship NuGet packages tagged `x.y.N-alpha`; stable SemVer is reserved for `v1.0.0` and later.

## Adding a new rule

The canonical flow, end to end:

1. **Declare the rule.** Create `src/XamlLint.Core/Rules/<Category>/LX###_DescriptiveName.cs`, a `public sealed partial class LX###_DescriptiveName : IXamlRule` with a `[XamlRule(...)]` attribute (id, upstreamId, title, default severity, dialect mask, help URI). The source generator picks it up automatically — no manual catalog registration.
2. **Add the unshipped entry.** Append a row to `AnalyzerReleases.Unshipped.md` under `### New Rules`, matching the `ID | Category | Severity | Notes` shape.
3. **Add fixtures and tests.** Create `tests/XamlLint.Core.Tests/Rules/<Category>/LX###_DescriptiveNameTest.cs` using `XamlDiagnosticVerifier<TRule>` with inline `[|...|]` / `{|LX###:...|}` span markers, or directory-per-rule fixtures under `tests/XamlLint.Core.Tests/Rules/<Category>/LX###/` when the scenario needs a full file.
4. **Run DocTool.** `dotnet run --project src/XamlLint.DocTool --configuration Release` stubs the `docs/rules/LX###.md` file with the 4-heading template and regenerates `docs/schema/v1/config.json` + presets.
5. **Fill in the rule doc.** Replace the TBD placeholders in the stubbed `docs/rules/LX###.md`. Follow the shape of an existing rule doc (inline-annotated XAML examples adjacent to the offending token — no separate "Bad / Good" section).
6. **Link from the category overview.** Add a table row linking to the new rule in `docs/rules/<category>.md`.
7. **Run meta-tests.** `dotnet test --solution xaml-lint.slnx --filter-method "*Meta*"` asserts catalog invariants (unique IDs, release-file consistency, doc existence, HelpUri pattern, dialect mask non-zero, filename matches ID).
8. **Run full CI check.** `dotnet run --project src/XamlLint.DocTool --configuration Release -- --check` must print "no drift".

## Running tests locally

```
dotnet test --solution xaml-lint.slnx --configuration Release --verbosity normal
```

For a filtered run:

```
dotnet test --solution xaml-lint.slnx --filter-method "*<TestName>*"
```

Test stack: xUnit v3 on Microsoft Testing Platform (`xunit.v3.mtp-v2`), AwesomeAssertions (MIT fork of FluentAssertions) for assertions. `dotnet test` with a positional path is ignored on MTP — always use `--solution xaml-lint.slnx`.

## Doc tool and CI checks

`XamlLint.DocTool` is a post-build step that stubs missing rule docs, deletes orphaned stubs (only when the file is clearly stub-shaped), regenerates `docs/schema/v1/config.json`, and regenerates presets from each rule's `DefaultSeverity`. CI runs it as a drift check:

```
dotnet run --project src/XamlLint.DocTool --configuration Release -- --check
```

A non-zero exit indicates the working tree is stale — re-run the tool without `--check` and commit the regenerated outputs.
```

- [ ] **Step 2: Link CONTRIBUTING.md from README**

In `README.md`, between the `## Attribution` and `## License` sections, add:

```markdown
## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the versioning policy, the "add a new rule" flow, and how to run tests locally.
```

- [ ] **Step 3: Build + tests still green**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green (no source changes).

- [ ] **Step 4: Commit**

```bash
git add CONTRIBUTING.md README.md
git commit -m "docs: add CONTRIBUTING.md with versioning policy and new-rule flow"
```

---

## Task 11: Source and commit the NuGet package icon

Produces `assets/icon.png` at 128×128 PNG per NuGet spec. User may supply their own icon at `assets/icon.png` (preferred) or invoke the PowerShell fallback below to generate a minimal glyph-based icon.

**Files:**
- Create: `assets/icon.png`

- [ ] **Step 1: Create the assets directory**

```bash
mkdir -p assets
```

- [ ] **Step 2a (user-supplied): commit the user's icon**

If the user has an icon ready, save it to `assets/icon.png` (128×128 PNG, under 1MB). Skip to Step 3.

- [ ] **Step 2b (fallback): generate a minimal placeholder icon with GDI+**

On Windows (the host for this repo per `global.json`/`CLAUDE.md`), run this PowerShell one-shot that writes a 128×128 PNG with a dark teal background and a white `</>` glyph:

```powershell
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap 128, 128
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$g.Clear([System.Drawing.Color]::FromArgb(40, 44, 52))
$font = New-Object System.Drawing.Font 'Segoe UI', 36, ([System.Drawing.FontStyle]::Bold)
$brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(97, 175, 239))
$fmt = New-Object System.Drawing.StringFormat
$fmt.Alignment = [System.Drawing.StringAlignment]::Center
$fmt.LineAlignment = [System.Drawing.StringAlignment]::Center
$rect = New-Object System.Drawing.RectangleF 0, 0, 128, 128
$g.DrawString('</>', $font, $brush, $rect, $fmt)
$bmp.Save('assets/icon.png', [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
```

Expected: a new `assets/icon.png` roughly 3–6 KB. Open in an image viewer to confirm a dark-teal square with a light-blue `</>` glyph rendered center.

- [ ] **Step 3: Verify file dimensions and size**

```bash
file assets/icon.png   # expect: PNG image data, 128 x 128, 8-bit/color RGBA
ls -lh assets/icon.png  # expect: well under 1MB
```

NuGet spec: PNG, 128×128, under 1MB. All three must hold.

- [ ] **Step 4: Commit**

```bash
git add assets/icon.png
git commit -m "assets: add 128x128 PNG icon for NuGet package"
```

---

## Task 12: Wire PackageIcon into XamlLint.Cli.csproj and verify pack output

Adds `<PackageIcon>` to the csproj and pack-includes the icon at the package root. Verifies the resulting nupkg has both `icon.png` and `README.md` at its root by inspecting the archive.

**Files:**
- Modify: `src/XamlLint.Cli/XamlLint.Cli.csproj`

- [ ] **Step 1: Add PackageIcon and pack the asset**

In `src/XamlLint.Cli/XamlLint.Cli.csproj`, add `<PackageIcon>icon.png</PackageIcon>` to the first `<PropertyGroup>` (alongside `<PackageReadmeFile>README.md</PackageReadmeFile>`), and add an icon pack-include line to the existing `<ItemGroup>` that packs the README:

```xml
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <RootNamespace>XamlLint.Cli</RootNamespace>
    <AssemblyName>xaml-lint</AssemblyName>

    <!-- dotnet tool packing -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>xaml-lint</ToolCommandName>
    <PackageId>xaml-lint</PackageId>
    <IsPackable>true</IsPackable>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>
    <Description>Command-line XAML linter. Install via `dotnet tool install -g xaml-lint`.</Description>
    <PackageTags>xaml;lint;wpf;winui;maui;avalonia;uno;analysis</PackageTags>
  </PropertyGroup>
```

```xml
  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="/" />
    <None Include="..\..\assets\icon.png" Pack="true" PackagePath="/" />
  </ItemGroup>
```

- [ ] **Step 2: Build and pack**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet pack src/XamlLint.Cli --configuration Release --no-build --output ./artifacts
```

Expected: a `.nupkg` file under `./artifacts/`, e.g., `./artifacts/xaml-lint.0.5.0-alpha.g<hash>.nupkg`.

- [ ] **Step 3: Inspect the nupkg contents**

```bash
ls ./artifacts/
# Take the nupkg name from the listing
unzip -l ./artifacts/xaml-lint.0.5.0-alpha*.nupkg | grep -E 'icon\.png|README\.md'
```

Expected output includes two lines, both showing the files at the archive root (no subdirectory path):

```
    ????  ????-??-?? ??:??   icon.png
    ????  ????-??-?? ??:??   README.md
```

If either file appears under a subfolder (e.g., `assets/icon.png`), the `PackagePath="/"` attribute on the `<None>` item is missing or wrong — fix and re-pack.

- [ ] **Step 4: Run tests**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green (csproj change is packaging-metadata only; no code affected).

- [ ] **Step 5: Commit**

```bash
git add src/XamlLint.Cli/XamlLint.Cli.csproj
git commit -m "build(cli): embed README and icon in NuGet package"
```

The artifact directory is ignored by `.gitignore`; do not commit `./artifacts/`.

---

## Task 13: Commit marketplace submission materials

Creates `docs/marketplace/` with the listing copy and a screenshots directory. The materials are the M5 deliverable per spec §2 point 5; the actual submission to the marketplace is deferred to the `v1.0.0` tag moment (or an M5-exit action if the user prefers).

**Files:**
- Create: `docs/marketplace/listing.md`
- Create: `docs/marketplace/screenshots/README.md`

- [ ] **Step 1: Write docs/marketplace/listing.md**

```markdown
# xaml-lint — marketplace listing

## Short description (one line, ~120 chars)

A Claude Code plugin that lints XAML files — catch Grid-layout, binding, naming, and deprecation issues as code is written.

## Long description

`xaml-lint` analyzes XAML files and reports common problems, so Claude catches them as it writes and edits views.

The plugin installs a `PostToolUse` hook that runs `xaml-lint` on every `.xaml` file Claude writes or edits; diagnostics land back in context automatically, without any prompt. A `/xaml-lint:lint` slash command and an on-demand skill handle the cases where you want to check files manually.

The rule catalog (20 IDs at v1) is derived from Matt Lacey's Rapid XAML Toolkit: Grid-layout sanity (`Grid.Row`/`Grid.Column` without matching definitions, spans exceeding available rows/columns), binding issues (`SelectedItem` should be `TwoWay`, prefer `x:Bind` on UWP/WinUI 3), naming (`x:Name` / `x:Uid` casing), resource-localization hints, input-control scope gaps, Slider/Stepper range checks, and deprecation warnings (`MediaElement` → `MediaPlayerElement`). Rules are dialect-gated; the WPF-primary, WinUI 3 / UWP / .NET MAUI rules only fire when those dialects are detected.

Output formats: `pretty` (ANSI, TTY default), `compact-json` (stable envelope, what the hook reads), `sarif` (SARIF 2.1.0 for CI), `msbuild` (one line per diagnostic, `dotnet build` style).

The analysis engine is stateless and AOT-friendly; a v2 LSP server is planned as a purely additive wrap.

## Feature bullets

- 20 XAML lint rules across Layout, Bindings, Naming, Resources, Input, and Deprecated categories
- Dialect-aware: WPF primary, with dialect-gated rules for WinUI 3, UWP, and .NET MAUI
- ReSharper-style suppression pragmas (`<!-- xaml-lint disable [once] RULE -->`)
- Configurable via `xaml-lint.config.json` with three bundled presets (`xaml-lint:off`, `xaml-lint:recommended`, `xaml-lint:strict`)
- Four output formats: `pretty`, `compact-json`, `sarif`, `msbuild`
- Source-generated rule catalog; per-rule `HelpUri` pointing at hosted docs
- Published to NuGet as a `dotnet tool`

## Requirements

- Claude Code (plugin host)
- .NET 10 SDK on `PATH`
- `dotnet tool install -g xaml-lint` after plugin enable

## Links

- Repo: https://github.com/jizc/xaml-lint
- Docs: https://jizc.github.io/xaml-lint/
- NuGet: https://www.nuget.org/packages/xaml-lint
- Changelog: https://github.com/jizc/xaml-lint/blob/main/CHANGELOG.md
```

- [ ] **Step 2: Write docs/marketplace/screenshots/README.md**

```markdown
# Screenshots for marketplace listing

Three PNGs expected before marketplace submission:

1. **`pretty-formatter.png`** — a terminal showing `xaml-lint lint src/**/*.xaml` output in `pretty` format. Goals: show colored headers, per-file grouping, aligned columns, and a concrete rule diagnostic with a short message.

2. **`claude-mid-edit.png`** — a Claude Code session showing the `PostToolUse` hook firing immediately after an `Edit` tool call on a `.xaml` file, with `xaml-lint` diagnostics reported back in the same turn. Goals: demonstrate the "Claude catches it as you write" value prop.

3. **`sarif-ci.png`** — GitHub Actions or Azure DevOps annotations panel showing `xaml-lint --format sarif` uploaded as a code-scanning result. Goals: show CI-facing use beyond the plugin surface.

Dimensions: capture at **1280×720** (16:9), PNG, each under 1 MB. If the marketplace submission flow later requires a different aspect ratio, re-capture and update.

If the submission flow requires a metadata sidecar (caption strings, alt text), record it alongside the PNGs as `captions.md` at submission time.
```

- [ ] **Step 3: Verify build + tests (no code changes, but confirm)**

```bash
dotnet build xaml-lint.slnx --configuration Release
dotnet test --solution xaml-lint.slnx --configuration Release --no-build --verbosity normal
```

Expected: green.

- [ ] **Step 4: Commit**

```bash
git add docs/marketplace/
git commit -m "docs(marketplace): add listing copy and screenshot spec"
```

---

## Task 14: Update parent spec and v1-status tracker

Executes the downstream doc updates in spec §6.1 and §6.2. Parent spec §10 gets an M5 block and the v1.0.0 entry collapses; §13 collapses to a one-liner. The tracker gets an M5 row and an "M5 deliverables" section replacing the §13 section.

**Files:**
- Modify: `docs/superpowers/specs/2026-04-17-xaml-lint-design.md`
- Modify: `docs/superpowers/v1-status.md`

- [ ] **Step 1: Insert M5 block in parent spec §10 (between M4 and v1.0.0)**

In `docs/superpowers/specs/2026-04-17-xaml-lint-design.md`, after the `### M4 — Dialect-gated rules (v0.4.0)` block (lines ~690–695) and before `### v1.0.0 release` (line ~696), insert:

```markdown
### M5 — Pre-v1 polish (v0.5.0)
- Close every parent-spec §13 gate in a dedicated milestone, rules-inert
- Phase A: decide and (if moving) migrate repo location
- Phase B: migrate schema + rule docs to GitHub Pages; update `HelpUri` scheme across rules and meta-test; update `$schema` URLs in presets, schema, README, and config reference
- Phase C: `CONTRIBUTING.md` (versioning policy + add-a-new-rule flow), NuGet package readme + 128×128 icon wired via `<PackageReadmeFile>` + `<PackageIcon>`, marketplace submission materials under `docs/marketplace/`
- No new rules, no behavior changes; catalog frozen at 20 IDs through v1.0.0
- `AnalyzerReleases.Unshipped.md` stays empty; no graduation to `Shipped.md` at tag time
- Full design: [specs/2026-04-18-m5-pre-v1-polish-design.md](2026-04-18-m5-pre-v1-polish-design.md)
```

- [ ] **Step 2: Collapse the v1.0.0 release entry in §10**

Replace the existing `### v1.0.0 release` block (lines ~696–703) with:

```markdown
### v1.0.0 release
- M5 complete; alpha suffix dropped from `version.json`
- Announcement post; plugin marketplace listing goes live
- Final meta-test sweep (HelpUri pattern, AnalyzerReleases invariants, doc existence)
- Tag push; NuGet graduates from `0.5.0-alpha` to `1.0.0`
- Versioning policy from M5's `CONTRIBUTING.md` applies from this point on
```

- [ ] **Step 3: Collapse §13 to a one-liner**

Replace the entire `## 13. Open items before v1 tag` section (lines ~809–816) with:

```markdown
## 13. Open items before v1 tag

These five items are now tracked as **M5 deliverables** in [specs/2026-04-18-m5-pre-v1-polish-design.md](2026-04-18-m5-pre-v1-polish-design.md) and in the [v1 status tracker](../v1-status.md). Heading preserved so existing links don't break.
```

- [ ] **Step 4: Update v1-status.md milestone table and deliverables section**

In `docs/superpowers/v1-status.md`:

- Line 6: change `| **Updated** | 2026-04-18 |` to today's date (keep in sync with when Task 14 lands).
- Keep `| **Current release** | [v0.4.0](...) |` until v0.5.0 actually ships.

In the Milestone progress table (around lines 14–22), insert an M5 row between the M4 row and the v1.0.0 row:

```markdown
| M5 — Pre-v1 polish | v0.5.0 | 🚧 In progress | Pages migration, `HelpUri` scheme change, `CONTRIBUTING.md`, NuGet readme+icon, marketplace materials. See [spec](specs/2026-04-18-m5-pre-v1-polish-design.md). |
```

In the v1.0.0 row, swap the Notes cell `See [§13 open items](#§13-open-items-remaining-before-v1-tag) below` for `Gates: M5 complete, alpha suffix dropped, announcement drafted.`

Replace the entire `## §13 open items (remaining before v1 tag)` section (around lines 39–49) with:

```markdown
## M5 deliverables

Direct mapping of M5 spec §2 — each row is one gate.

| # | Item | Phase | Status | Notes |
|---|---|---|---|---|
| 1 | Decide repo location (stay on `jizc/` vs move to an org) | A | ⬜ Not started | Default: stay on `jizc/`. Migrate before any Phase B URL edit to avoid a double URL sweep. |
| 2 | Migrate schema + rule docs to GitHub Pages | B | ⬜ Not started | Moves `schema/v1/` under `docs/`, adds Jekyll scaffold, flips `HelpUri` + `$schema` URLs. |
| 3 | Write `CONTRIBUTING.md` | C | ⬜ Not started | Versioning policy (spec §10) + "add a new rule" flow + how to run tests + DocTool usage. |
| 4 | NuGet package readme + icon | C | ⬜ Not started | `<PackageReadmeFile>` already wired; add `<PackageIcon>` and commit `assets/icon.png` (128×128 PNG). |
| 5 | Plugin marketplace submission materials | C | ⬜ Not started | Listing copy + screenshot spec under `docs/marketplace/`. Submission itself may defer to v1.0.0. |
```

- [ ] **Step 5: Run tests as sanity check**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --verbosity normal
```

Expected: green (doc-only changes).

- [ ] **Step 6: Commit**

```bash
git add docs/superpowers/specs/2026-04-17-xaml-lint-design.md docs/superpowers/v1-status.md
git commit -m "docs(superpowers): fold M5 into parent spec and v1 tracker"
```

---

## Task 15: Update CHANGELOG, bump plugin manifest, sanity-check AnalyzerReleases

Release-wrap-up edits done just before the tag. Adds the `[0.5.0]` CHANGELOG section per spec §6.3, bumps the plugin manifest version, and confirms `AnalyzerReleases.Unshipped.md` has no rule rows (since M5 adds no rules, no graduation to `Shipped.md` happens).

**Files:**
- Modify: `CHANGELOG.md`
- Modify: `.claude-plugin/plugin.json`
- Verify (no changes expected): `AnalyzerReleases.Unshipped.md`, `AnalyzerReleases.Shipped.md`

- [ ] **Step 1: Bump plugin manifest version**

In `.claude-plugin/plugin.json:3`, change `"version": "0.4.0"` to `"version": "0.5.0"`. Everything else stays the same:

```json
{
  "name": "xaml-lint",
  "version": "0.5.0",
  "description": "Lints XAML files for common issues so Claude can catch problems as code is written.",
  "author": {
    "name": "Jan Ivar Z. Carlsen"
  },
  "license": "MIT",
  "homepage": "https://github.com/jizc/xaml-lint",
  "keywords": ["xaml", "lint", "wpf", "winui", "maui", "avalonia"]
}
```

- [ ] **Step 2: Add the [0.5.0] CHANGELOG section**

In `CHANGELOG.md`, insert a new section between `## [Unreleased]` (line 9) and `## [0.4.0] - 2026-04-18` (line 11). Use today's date:

```markdown
## [0.5.0] - YYYY-MM-DD

M5 — pre-v1 polish. Rules-inert release focused on hosting, release-surface polish, and contributor docs.

### Added

- `CONTRIBUTING.md` with versioning policy and the "add a new rule" flow ([#6])
- NuGet package icon at `assets/icon.png` (128×128 PNG), wired via `<PackageIcon>` ([#6])
- Marketplace submission materials under `docs/marketplace/` (listing copy + screenshot spec) ([#6])
- GitHub Pages scaffold at `docs/_config.yml`; schema and rule docs now served at `https://jizc.github.io/xaml-lint/` ([#6])

### Changed

- `HelpUri` scheme flipped from `github.com/jizc/xaml-lint/blob/main/docs/rules/LX###.md` to `jizc.github.io/xaml-lint/rules/LX###` across all 26 call sites (20 rule attributes + 3 constants + 3 inline) ([#6])
- `$schema` / `$id` URLs in `docs/schema/v1/config.json`, the three presets, `README.md`, and `docs/config-reference.md` now point at the Pages host ([#6])
- `schema/v1/` tree moved under `docs/schema/v1/` so GitHub Pages can serve it from the `/docs` source folder ([#6])
- Rule docs and category overviews now carry minimal YAML front matter for Jekyll rendering ([#6])
```

Also update the `[Unreleased]` compare link and add a new `[0.5.0]` compare link at the bottom of the file. Replace:

```
[Unreleased]: https://github.com/jizc/xaml-lint/compare/v0.4.0...HEAD
```

with:

```
[Unreleased]: https://github.com/jizc/xaml-lint/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/jizc/xaml-lint/compare/v0.4.0...v0.5.0
```

Add `[#6]: https://github.com/jizc/xaml-lint/pull/6` (or whatever PR number this milestone uses) to the footer link block.

Fill in the `YYYY-MM-DD` date with today's date when actually committing.

- [ ] **Step 3: Verify AnalyzerReleases.Unshipped.md is empty of rule rows**

```bash
rg '^LX\d{3}' AnalyzerReleases.Unshipped.md
```

Expected: **zero matches**. The file should only contain the `; Unshipped analyzer release` header and the empty `### New Rules` + table header. No rule rows were added in M5.

If this check fails, a rule leaked in somewhere — halt and diagnose; M5 spec §1 explicitly states no new rules.

- [ ] **Step 4: Run meta-tests one more time**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --filter-method "*Meta*"
```

Expected: green. This covers HelpUri pattern, AnalyzerReleases referential integrity, doc existence, dialect mask non-zero, filename-matches-id, deprecated-rules-have-replacement, and category-overview linkage.

- [ ] **Step 5: Run full test suite**

```bash
dotnet test --solution xaml-lint.slnx --configuration Release --verbosity normal
```

Expected: all green.

- [ ] **Step 6: Commit**

```bash
git add CHANGELOG.md .claude-plugin/plugin.json
git commit -m "release: prepare v0.5.0 — changelog, plugin manifest bump"
```

---

## Task 16: Merge and tag v0.5.0

Lands the M5 branch on `main` and tags `v0.5.0`. The release workflow (`.github/workflows/release.yml`) fires on the tag and publishes the nupkg as `0.5.0-alpha` to NuGet.org.

**Files:** no code changes. Git operations + CI verification.

- [ ] **Step 1: Push the branch and open the PR**

```bash
git push -u origin m5-pre-v1-polish
gh pr create --title "M5 — pre-v1 polish (v0.5.0)" --body "$(cat <<'EOF'
## Summary

- Phase A: repo-location decision recorded (stay on jizc/)
- Phase B: GitHub Pages migration — HelpUri scheme flipped, schema + rule docs under docs/, Pages serves at jizc.github.io/xaml-lint/
- Phase C: CONTRIBUTING.md, NuGet package icon, marketplace submission materials

Closes every item in parent spec §13. Rules-inert: no new lint rules, no behavior changes.

Full design: docs/superpowers/specs/2026-04-18-m5-pre-v1-polish-design.md
Plan: docs/superpowers/plans/2026-04-18-m5-pre-v1-polish.md

## Test plan

- [x] dotnet build xaml-lint.slnx --configuration Release (green on all three OSes)
- [x] dotnet test --solution xaml-lint.slnx (green — meta-tests and full suite)
- [x] dotnet run --project src/XamlLint.DocTool --configuration Release -- --check (no drift)
- [x] Pages smoke test — rule docs, schema, and all three presets return 200 at Pages URLs
- [x] dotnet pack src/XamlLint.Cli — nupkg contains README.md and icon.png at root
EOF
)"
```

- [ ] **Step 2: Wait for CI to go green**

```bash
gh pr checks --watch
```

Expected: all three OS matrix jobs green (build, test, DocTool --check).

- [ ] **Step 3: Merge to main**

Merge via the GitHub web UI or:

```bash
gh pr merge --squash --delete-branch
```

- [ ] **Step 4: Tag v0.5.0**

```bash
git checkout main
git pull origin main
git tag v0.5.0
git push origin v0.5.0
```

The release workflow `.github/workflows/release.yml` triggers on `refs/tags/v*` and runs: build → test → DocTool --check → pack → `dotnet nuget push` with skip-duplicate.

- [ ] **Step 5: Verify the release workflow succeeded**

```bash
gh run list --workflow=release.yml --limit 1
gh run view --log-failed  # only needed if failed
```

Expected: most recent run on tag `v0.5.0` is green.

Verify on NuGet.org that `xaml-lint 0.5.0-alpha` (or `0.5.0-alpha.<height>`) appears at https://www.nuget.org/packages/xaml-lint.

- [ ] **Step 6: Back-update v1-status.md tracker post-tag**

One follow-up edit to the tracker once the tag is live (can be a separate commit on main):

- Status row M5: `🚧 In progress` → `✅ Done`
- Current release: `v0.4.0` → `v0.5.0`
- Updated date: refresh

```bash
# Edit docs/superpowers/v1-status.md per above
git add docs/superpowers/v1-status.md
git commit -m "docs(tracker): mark M5 done after v0.5.0 ship"
git push origin main
```

M5 complete. Next stop: v1.0.0 (graduation tag — drop alpha suffix from `version.json`, final meta-test sweep, announcement, marketplace submission goes live).

---

## Self-review checklist

Spec coverage verified against `docs/superpowers/specs/2026-04-18-m5-pre-v1-polish-design.md`:

- §2 gate 1 (repo location decision) → Task 2 ✓
- §2 gate 2 (GitHub Pages migration) → Tasks 3, 4, 5, 6, 7, 8, 9 ✓
- §2 gate 3 (`CONTRIBUTING.md`) → Task 10 ✓
- §2 gate 4 (NuGet readme + icon) → Tasks 11, 12 ✓
- §2 gate 5 (marketplace materials) → Task 13 ✓
- §3 Phase A (D1 + optional migration) → Tasks 1, 2 ✓
- §3 Phase B (URL migration, steps 3–10) → Tasks 3–9 ✓
- §3 Phase C (release polish, steps 11–14) → Tasks 10–13 ✓
- §4 D1 default (stay on `jizc/`) → baked into Task 2 ✓
- §4 D2 default (extensionless rule URLs) → baked into Task 5, 6, 9 URL shape ✓
- §4 D3 (icon source) → Task 11 provides user-supplied OR PowerShell fallback ✓
- §4 D4 (listing tone matches README) → Task 13 listing.md tone ✓
- §5 exit criteria checklist → addressed across Tasks 2, 9, 12, 10, 13, 15, 16 ✓
- §6.1 parent spec updates → Task 14 ✓
- §6.2 tracker updates → Task 14 ✓
- §6.3 CHANGELOG at tag time → Task 15 ✓
- §6.4 files not touched (AnalyzerReleases, rule source bodies, test source except meta regex) → respected ✓
- §7 risks: D1 gates Phase B (Tasks 1–2 precede Task 3) ✓; Jekyll front matter addressed (Task 4) ✓; icon size verified (Task 11 Step 3) ✓; marketplace submission decoupled from M5 exit (Task 13 is materials-only) ✓

No placeholders, no TBDs, no "handle edge cases" — every step shows the command, the diff, or the file contents.
