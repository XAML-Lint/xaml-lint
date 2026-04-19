# Contributing to xaml-lint

Thanks for considering a contribution. This project is a Claude Code plugin that lints XAML files; the primary consumer is Claude itself, but contributions that make it better for human and CI use are equally welcome.

## Versioning policy

`xaml-lint` follows [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html). For rule-catalog changes specifically:

- **Rule additions** → minor version bump.
- **Rule removals** → major version bump. Deprecated rules stay in the catalog with `Deprecated = true` and (usually) `ReplacedBy = "LX###"` pointing at a successor; they are not deleted.
- **Severity downgrades** (e.g., `warning` → `info`) → minor version bump.
- **Severity upgrades** (e.g., `warning` → `error`) → major version bump.

Versions are declared verbatim in `version.json` (3-segment, no prerelease suffix). Cutting a release is three edits — bump `version.json`, graduate `AnalyzerReleases.Unshipped.md` into a new `## Release x.y.z` section in `Shipped.md`, move `[Unreleased]` into a new `[x.y.z]` section in `CHANGELOG.md` — then commit, tag `vx.y.z`, and push.

## Adding a new rule

The canonical flow, end to end:

1. **Declare the rule.** Create `src/XamlLint.Core/Rules/<Category>/LX###_DescriptiveName.cs`, a `public sealed partial class LX###_DescriptiveName : IXamlRule` with a `[XamlRule(...)]` attribute (id, upstreamId, title, default severity, dialect mask, help URI). The source generator picks it up automatically — no manual catalog registration.
2. **Add the unshipped entry.** Append a row to `AnalyzerReleases.Unshipped.md` under `### New Rules`, matching the `ID | Category | Severity | Notes` shape.
3. **Add fixtures and tests.** Create `tests/XamlLint.Core.Tests/Rules/<Category>/LX###_DescriptiveNameTest.cs` using `XamlDiagnosticVerifier<TRule>` with inline `[|...|]` / `{|LX###:...|}` span markers, or directory-per-rule fixtures under `tests/XamlLint.Core.Tests/Rules/<Category>/LX###/` when the scenario needs a full file.
4. **Run DocTool.** `dotnet run --project src/XamlLint.DocTool --configuration Release` stubs the `docs/rules/LX###.md` file with the 4-heading template and regenerates `schema/v1/config.json` + presets.
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

`XamlLint.DocTool` is a console tool that stubs missing rule docs, deletes orphaned stubs (only when the file is clearly stub-shaped), regenerates `schema/v1/config.json`, and regenerates presets from each rule's `DefaultSeverity`. It is not wired into `dotnet build` — run it explicitly whenever you add or change a rule. CI runs it as a drift check:

```
dotnet run --project src/XamlLint.DocTool --configuration Release -- --check
```

A non-zero exit indicates the working tree is stale — re-run the tool without `--check` and commit the regenerated outputs.
