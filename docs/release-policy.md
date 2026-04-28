# Release policy

Durable rules that govern how xaml-lint releases happen. Independent of what's in any given release — see [`backlog.md`](backlog.md) for ideas under consideration.

## Cadence

- **No calendar commitments.** Release windows are "when ready + dogfood clean."
- **Bias toward fewer, larger themed releases.** Release admin (changelog, version bumps, NuGet publish, dogfood sweeps, release notes) dominates the marginal shipping cost, so prefer bundling related work into one themed release over a chain of trivial minors.

## Versioning

- **Pre-adoption posture.** Breaking changes ship as minors. While xaml-lint has no users yet, a major bump per breaking change isn't earning anything.
- **Adoption tightens this.** When real consumers materialize, the policy shifts to strict semver — breaking changes force majors again.

## Release gate

- Each version bump runs the dogfood corpus sweep from [`dogfooding.md`](dogfooding.md) for each dialect whose rules changed.
- **Baseline diff is the gate, not "did tests pass."** Any unexpected delta on the corpus is a blocker; only intentional, explained deltas ship.

## CHANGELOG scope

- User-visible changes only. Build / tooling churn — internal refactors, CI tweaks, version-tooling swaps — stays in git history, not release notes.
- Rule-level history is tracked separately in [`AnalyzerReleases.Shipped.md`](../AnalyzerReleases.Shipped.md).
