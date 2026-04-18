# M4 — Dialect-Gated Rules Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. One commit per task, each standalone. After each task: `dotnet build xaml-lint.slnx --configuration Release` clean and `dotnet test --solution xaml-lint.slnx` green. Run `dotnet run --project src/XamlLint.DocTool -- --check` after tasks touching rules or docs to confirm no doc drift.

**Goal:** Ship the six dialect-gated lint rules on top of v0.3.0: `LX201` (prefer x:Bind), `LX301` (x:Uid casing), `LX500` (TextBox InputScope), `LX501` (Slider Minimum > Maximum), `LX502` (Stepper Minimum > Maximum), `LX600` (MediaElement deprecated). Tag **v0.4.0** with docs, presets, schema, AnalyzerReleases, and the RXT comparison all regenerated and in sync. First milestone to exercise the dispatcher's `Dialects` mask gating and to introduce the `Input` (LX5xx) and `Deprecated` (LX6xx) categories.

**Architecture:** Every rule declares its target dialects in its `[XamlRule(Dialects = …)]` attribute; `RuleDispatcher` filters by a bitwise AND before invoking the rule, so rule bodies assume the gate has already run. Five of the six rules are plain attribute/element scans on top of M2's `MarkupExtensionHelpers` + `XamlNamespaces` + `LocationHelpers`; the Slider/Stepper pair shares a tiny `NumericRangeHelpers` helper for literal-double parsing and span computation on the winning attribute. Each rule lands as its own commit with tests and an `AnalyzerReleases.Unshipped.md` row.

**Tech Stack:** .NET 10, `System.Xml.Linq` (`XDocument` with `LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace` from M1), the M1 source generator (`XamlLint.Core.SourceGen`), the M1 `XamlLint.DocTool` (stubs docs, writes presets, writes schema; `--check` in CI), xUnit v3 + Microsoft Testing Platform + AwesomeAssertions for tests using the M1 `XamlDiagnosticVerifier<TRule>` marker harness with its dialect-selecting overload, `Nerdbank.GitVersioning` for the `0.3-alpha` → `v0.4.0` bump.

---

## Notes before starting

**Test harness reminders (from M2/M3).**

1. `XamlDiagnosticVerifier<TRule>.Analyze(markedSource, Dialect dialect = Dialect.Wpf)` loads the marked string, strips `[|…|]` / `{|…|}` markers, and asserts the rule's diagnostic spans match exactly what's marked. Pass a non-default dialect when the rule under test only applies to UWP/WinUI 3/MAUI. The harness routes through the real `RuleDispatcher`, so rules whose `Dialects` mask excludes the passed dialect emit zero diagnostics — exactly what M4's "wrong dialect" tests rely on.

2. `[|…|]` is shorthand for "expect this rule's diagnostic here"; `{|LXNNN:…|}` spells out the rule ID (not needed when each test file pins one rule). `{|LXNNN|}` (span-less) means "expect any diagnostic for this rule somewhere in the source" — not used in M4.

3. Raw triple-quoted strings (`""" ... """`) keep the markers aligned to the source columns. Do not replace with regular `"..."` strings: the column-math in the harness relies on the raw layout.

**Source-generator contract.** Declaring a `public sealed partial class LXnnn_Foo : IXamlRule` with a `[XamlRule(...)]` attribute is sufficient to have it picked up by `XamlLint.Core.SourceGen` at build time. The generator emits a `Metadata` property and adds the class to `GeneratedRuleCatalog.Rules`. No manual registration.

**Dialect gating.** Every rule body in M4 assumes the dispatcher has already filtered by dialect. Do not add a defensive `if (context.Dialect != Dialect.WinUI3) yield break;` — it duplicates the dispatcher's check and the meta-tests treat a missing `Dialects` mask entry as a bug. The bitwise form is `Dialects = Dialect.Uwp | Dialect.WinUI3` (multi-dialect) or `Dialects = Dialect.Maui` (single-dialect). `Dialect.All` is wrong for M4 — every M4 rule narrows its mask.

**Tool rules vs lint rules.** Tool rules (LX001–LX006) implement `IToolRule` and are skipped by the dispatcher. M4 rules are lint rules; they must **not** implement `IToolRule`.

**Package versions.** M4 does not add any NuGet packages. Central package management in `Directory.Packages.props` already pins every dependency needed.

**Test runner invocation.** `dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx` (MTP ignores `dotnet test` positional paths; `--solution` is mandatory on .NET 10). For a filtered run: `dotnet test --solution … --filter-method "*TestName*"`.

**Existing files that M4 must not destroy.** Every rule file under `src/XamlLint.Core/Rules/{Tool,Bindings,Naming,Resources,Layout}/`, every doc file under `docs/rules/` (LX001.md–LX006.md, LX100.md–LX104.md, LX200.md, LX300.md, LX400.md, the category overview pages `tool.md`, `bindings.md`, `naming.md`, `resources.md`, `layout.md`, and `comparison-with-rapid-xaml-toolkit.md`), all `AnalyzerReleases.Shipped.md` sections (0.1.0, 0.2.0, 0.3.0), `CHANGELOG.md` entries for prior releases, the `schema/v1/presets/*.json` (these get regenerated by DocTool — do not hand-edit), all M1/M2/M3 helpers (`XamlNamespaces.cs`, `LocationHelpers.cs`, `MarkupExtensionHelpers.cs`, `GridAncestryHelpers.cs`), the M1 test harness under `tests/XamlLint.Core.Tests/TestInfrastructure/`.

---

## File structure

**New files created in M4:**

```
src/XamlLint.Core/
  Helpers/
    NumericRangeHelpers.cs           # literal-double parse + winning-attribute picker for LX501/LX502
  Rules/
    Bindings/
      LX201_PreferXBind.cs
    Naming/
      LX301_XUidCasing.cs
    Input/                           # new category directory
      LX500_TextBoxWithoutInputScope.cs
      LX501_SliderMinimumGreaterThanMaximum.cs
      LX502_StepperMinimumGreaterThanMaximum.cs
    Deprecated/                      # new category directory
      LX600_MediaElementDeprecated.cs

tests/XamlLint.Core.Tests/
  Helpers/
    NumericRangeHelpersTest.cs
  Rules/
    Bindings/
      LX201_PreferXBindTest.cs
    Naming/
      LX301_XUidCasingTest.cs
    Input/                           # new category directory
      LX500_TextBoxWithoutInputScopeTest.cs
      LX501_SliderMinimumGreaterThanMaximumTest.cs
      LX502_StepperMinimumGreaterThanMaximumTest.cs
    Deprecated/                      # new category directory
      LX600_MediaElementDeprecatedTest.cs

docs/rules/
  LX201.md                           # stubbed by DocTool after Task 2, authored in Task 9
  LX301.md                           # stubbed by DocTool after Task 3, authored in Task 9
  LX500.md                           # stubbed by DocTool after Task 4, authored in Task 9
  LX501.md                           # stubbed by DocTool after Task 6, authored in Task 9
  LX502.md                           # stubbed by DocTool after Task 7, authored in Task 9
  LX600.md                           # stubbed by DocTool after Task 8, authored in Task 9
  input.md                           # category overview, Task 10
  deprecated.md                      # category overview, Task 10
```

**Files modified in M4:**

- `version.json` — Task 1 (`0.3-alpha` → `0.4-alpha`).
- `AnalyzerReleases.Unshipped.md` — Tasks 2/3/4/6/7/8 (one row per rule) and Task 13 (drain to Shipped).
- `AnalyzerReleases.Shipped.md` — Task 13 (add `## Release 0.4.0` section).
- `schema/v1/presets/*.json` — rewritten by DocTool (Tasks 2/3/4/6/7/8). Do not hand-edit.
- `schema/v1/config.json` — rewritten by DocTool (Tasks 2/3/4/6/7/8). Do not hand-edit.
- `docs/comparison-with-rapid-xaml-toolkit.md` — Task 11 (add rows for the six new rules + clarify dialect-scoped behavior notes).
- `README.md` — Task 11 (bump status line to v0.4.0; move WinUI 3 / UWP / .NET MAUI to "Partial" in the support matrix).
- `CHANGELOG.md` — Task 11 (add `## [0.4.0]` section and link-collection footer).
- `.claude-plugin/plugin.json` — Task 13 (bump manifest version to 0.4.0).

**Rule responsibilities (one-liners):**

- `LX201_PreferXBind` — flags any attribute whose value is `{Binding …}` on UWP/WinUI 3 and suggests `{x:Bind}`.
- `LX301_XUidCasing` — mirror of LX300 for `x:Uid`; flags when the first character isn't uppercase. UWP/WinUI 3 only.
- `LX500_TextBoxWithoutInputScope` — flags `<TextBox>` elements that lack an `InputScope` attribute on UWP/WinUI 3.
- `LX501_SliderMinimumGreaterThanMaximum` — flags `<Slider>` elements where the literal `Minimum` is greater than the literal `Maximum`. WPF and MAUI.
- `LX502_StepperMinimumGreaterThanMaximum` — mirror of LX501 for MAUI's `<Stepper>` control. MAUI only.
- `LX600_MediaElementDeprecated` — flags `<MediaElement>` elements on UWP/WinUI 3 and suggests `<MediaPlayerElement>`.

**Helper responsibilities (one-liners):**

- `NumericRangeHelpers.TryReadLiteralDouble(XAttribute?)` — returns the parsed `double` for a literal-valued attribute (invariant culture), or `null` when the attribute is absent, empty, or a markup extension. Skipping markup extensions is what keeps LX501/LX502 quiet on data-bound `Minimum`/`Maximum`.

---

## Task 1: Create branch and bump version

Starts M4 work on its own branch and bumps Nerdbank.GitVersioning's base version so every development build on this branch produces `0.4.0-alpha.N` and the eventual `v0.4.0` tag graduates to stable.

**Files:**
- Modify: `version.json`

- [ ] **Step 1: Create and check out the branch**

```bash
git -C D:/GitHub/jizc/xaml-lint checkout -b m4-dialect-gated-rules
git -C D:/GitHub/jizc/xaml-lint branch --show-current
```

Expected: `m4-dialect-gated-rules`.

- [ ] **Step 2: Bump `version.json`**

Edit `version.json`. Replace `"version": "0.3-alpha"` with `"version": "0.4-alpha"`. Full expected file contents:

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/main/src/NerdBank.GitVersioning/version.schema.json",
  "version": "0.4-alpha",
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

- [ ] **Step 3: Verify the solution still builds**

```bash
dotnet build D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --configuration Release
```

Expected: `Build succeeded.` with zero warnings (solution has `TreatWarningsAsErrors=true` in `Directory.Build.props`).

- [ ] **Step 4: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add version.json
git -C D:/GitHub/jizc/xaml-lint commit -m "chore: bump version to 0.4-alpha for M4"
```

---

## Task 2: LX201 — Prefer x:Bind over Binding

Adds the first M4 rule. Flags any attribute whose value is a `{Binding …}` markup extension on UWP/WinUI 3 files and suggests `{x:Bind}`. Non-binding extensions (`{StaticResource …}`, `{TemplateBinding …}`, literal values, `{x:Bind …}` itself) are ignored. Uses the existing `MarkupExtensionHelpers.TryParseExtension` from M2 — no new helpers.

**Files:**
- Create: `src/XamlLint.Core/Rules/Bindings/LX201_PreferXBind.cs`
- Create: `tests/XamlLint.Core.Tests/Rules/Bindings/LX201_PreferXBindTest.cs`
- Modify: `AnalyzerReleases.Unshipped.md` (add LX201 row)

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Rules/Bindings/LX201_PreferXBindTest.cs`:

```csharp
using XamlLint.Core.Rules.Bindings;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Bindings;

public sealed class LX201_PreferXBindTest
{
    [Fact]
    public void Binding_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label}"|] />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Binding_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label}"|] />
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void Binding_on_Wpf_is_not_flagged()
    {
        // LX201 targets UWP/WinUI 3 only; the dispatcher's Dialects-mask gate filters WPF out
        // before the rule even runs, so no diagnostic is emitted.
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{Binding Label}" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void XBind_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    Content="{x:Bind Label}" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void StaticResource_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{StaticResource ButtonLabel}" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TemplateBinding_is_not_flagged()
    {
        // TemplateBinding is already the ControlTemplate-optimal form; x:Bind is not meant to
        // replace it.
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{TemplateBinding Label}" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Literal_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="Hello" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_bindings_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Content="{Binding First}"|] />
                <Button [|Content="{Binding Second}"|] />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Binding_with_nested_converter_braces_is_still_flagged()
    {
        // The nested {StaticResource C} must not confuse the outer-extension detector.
        XamlDiagnosticVerifier<LX201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label, Converter={StaticResource C}}"|] />
            """,
            Dialect.WinUI3);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX201_PreferXBindTest*"
```

Expected: build error — `LX201_PreferXBind` does not exist.

- [ ] **Step 3: Implement the rule**

Create `src/XamlLint.Core/Rules/Bindings/LX201_PreferXBind.cs`:

```csharp
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Bindings;

[XamlRule(
    Id = "LX201",
    UpstreamId = "RXT170",
    Title = "Prefer x:Bind over Binding",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX201.md")]
public sealed partial class LX201_PreferXBind : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (!MarkupExtensionHelpers.TryParseExtension(attr.Value, out var ext)) continue;
                if (ext.Name != "Binding") continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"Prefer {{x:Bind}} over {{Binding}} on {context.Dialect}; compiled bindings are faster and validated at build time.",
                    File: document.FilePath,
                    StartLine: span.StartLine,
                    StartCol: span.StartCol,
                    EndLine: span.EndLine,
                    EndCol: span.EndCol,
                    HelpUri: Metadata.HelpUri);
            }
        }
    }
}
```

- [ ] **Step 4: Append the LX201 row to `AnalyzerReleases.Unshipped.md`**

Edit `AnalyzerReleases.Unshipped.md`. Append the LX201 row to the empty table. Full expected file:

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX201   | Bindings | Info     | Prefer x:Bind over Binding
```

- [ ] **Step 5: Run the tests**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX201_PreferXBindTest*"
```

Expected: all 9 LX201 tests pass.

- [ ] **Step 6: Run the full test suite and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release
```

Expected: all tests pass. DocTool writes a stub `docs/rules/LX201.md` (will be authored in Task 9), updates `schema/v1/config.json` and the three `schema/v1/presets/*.json`.

- [ ] **Step 7: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Rules/Bindings/LX201_PreferXBind.cs tests/XamlLint.Core.Tests/Rules/Bindings/LX201_PreferXBindTest.cs AnalyzerReleases.Unshipped.md docs/rules/LX201.md schema/
git -C D:/GitHub/jizc/xaml-lint commit -m "feat: add LX201 — Prefer x:Bind over Binding"
```

---

## Task 3: LX301 — x:Uid should start with uppercase

Mirror of LX300 for `x:Uid`. Same XAML-namespace guard, same uppercase-first-character check. Dialects narrow to UWP/WinUI 3 because `x:Uid` is meaningful as a resource-lookup key only on those platforms (WPF has no equivalent resource-lookup mechanism).

**Files:**
- Create: `src/XamlLint.Core/Rules/Naming/LX301_XUidCasing.cs`
- Create: `tests/XamlLint.Core.Tests/Rules/Naming/LX301_XUidCasingTest.cs`
- Modify: `AnalyzerReleases.Unshipped.md` (add LX301 row)

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Rules/Naming/LX301_XUidCasingTest.cs`:

```csharp
using XamlLint.Core.Rules.Naming;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Naming;

public sealed class LX301_XUidCasingTest
{
    [Fact]
    public void Lowercase_x_Uid_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="loginButton"|] />
            </Grid>
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void Lowercase_x_Uid_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="loginButton"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Lowercase_x_Uid_on_Wpf_is_not_flagged()
    {
        // x:Uid has no meaningful runtime behavior on WPF; the rule's Dialects mask filters it.
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="loginButton" />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Uppercase_x_Uid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="LoginButton" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Uid_attribute_without_x_prefix_is_ignored()
    {
        // Only x:Uid (the XAML 2006/2009 ns) is checked; unprefixed Uid has no framework
        // meaning on UWP/WinUI 3.
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Uid="lowercase" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Empty_x_Uid_is_ignored()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="one"|] />
                <Button [|x:Uid="two"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX301_XUidCasingTest*"
```

Expected: build error — `LX301_XUidCasing` does not exist.

- [ ] **Step 3: Implement the rule**

Create `src/XamlLint.Core/Rules/Naming/LX301_XUidCasing.cs`:

```csharp
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Naming;

[XamlRule(
    Id = "LX301",
    UpstreamId = "RXT451",
    Title = "x:Uid should start with uppercase",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX301.md")]
public sealed partial class LX301_XUidCasing : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (attr.Name.LocalName != "Uid") continue;
                if (!XamlNamespaces.IsXamlNamespace(attr.Name.NamespaceName)) continue;

                var value = attr.Value;
                if (value.Length == 0) continue;
                if (char.IsUpper(value[0])) continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"x:Uid '{value}' should start with an uppercase letter.",
                    File: document.FilePath,
                    StartLine: span.StartLine,
                    StartCol: span.StartCol,
                    EndLine: span.EndLine,
                    EndCol: span.EndCol,
                    HelpUri: Metadata.HelpUri);
            }
        }
    }
}
```

- [ ] **Step 4: Append the LX301 row to `AnalyzerReleases.Unshipped.md`**

Edit `AnalyzerReleases.Unshipped.md`. Rows are sorted by ID. Full expected state:

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX201   | Bindings | Info     | Prefer x:Bind over Binding
LX301   | Naming   | Warning  | x:Uid should start with uppercase
```

- [ ] **Step 5: Run the tests and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX301_XUidCasingTest*"
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release
```

Expected: all tests pass. DocTool stubs `docs/rules/LX301.md` and updates schema/presets.

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Rules/Naming/LX301_XUidCasing.cs tests/XamlLint.Core.Tests/Rules/Naming/LX301_XUidCasingTest.cs AnalyzerReleases.Unshipped.md docs/rules/LX301.md schema/
git -C D:/GitHub/jizc/xaml-lint commit -m "feat: add LX301 — x:Uid should start with uppercase"
```

---

## Task 4: LX500 — TextBox lacks InputScope

Opens the `Input` category (LX5xx). Flags UWP/WinUI 3 `<TextBox>` elements that don't set an `InputScope`. `InputScope` is the UWP/WinUI hint for soft-keyboard layout and IME behavior; unset, the user gets the default layout, which is rarely the right thing for numeric/URL/email inputs. No new helpers needed — it's an element-name filter plus an attribute-absence check.

**Files:**
- Create: `src/XamlLint.Core/Rules/Input/LX500_TextBoxWithoutInputScope.cs`
- Create: `tests/XamlLint.Core.Tests/Rules/Input/LX500_TextBoxWithoutInputScopeTest.cs`
- Modify: `AnalyzerReleases.Unshipped.md` (add LX500 row)

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Rules/Input/LX500_TextBoxWithoutInputScopeTest.cs`:

```csharp
using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX500_TextBoxWithoutInputScopeTest
{
    [Fact]
    public void TextBox_without_InputScope_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
            </Grid>
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Wpf_is_not_flagged()
    {
        // InputScope is a UWP/WinUI concept — meaningless on WPF.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_InputScope_attribute_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox InputScope="Number" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_with_InputScope_binding_is_not_flagged()
    {
        // A bound InputScope is still an "I know what I'm doing" signal; don't second-guess it.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox InputScope="{Binding Scope}" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBlock_is_not_flagged()
    {
        // TextBlock is not an input control.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock Text="read-only" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
                <[|TextBox|] />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX500_TextBoxWithoutInputScopeTest*"
```

Expected: build error — `LX500_TextBoxWithoutInputScope` does not exist; the `Input` namespace doesn't exist.

- [ ] **Step 3: Implement the rule**

Create `src/XamlLint.Core/Rules/Input/LX500_TextBoxWithoutInputScope.cs`:

```csharp
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX500",
    UpstreamId = "RXT150",
    Title = "TextBox lacks InputScope",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX500.md")]
public sealed partial class LX500_TextBoxWithoutInputScope : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "TextBox") continue;
            if (element.Attribute("InputScope") is not null) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "TextBox should set InputScope to hint the on-screen keyboard and IME behavior.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
```

- [ ] **Step 4: Append the LX500 row to `AnalyzerReleases.Unshipped.md`**

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX201   | Bindings | Info     | Prefer x:Bind over Binding
LX301   | Naming   | Warning  | x:Uid should start with uppercase
LX500   | Input    | Info     | TextBox lacks InputScope
```

- [ ] **Step 5: Run the tests and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX500_TextBoxWithoutInputScopeTest*"
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release
```

Expected: LX500 tests pass. The `Every_rule_appears_in_its_category_overview_page` meta-test currently fails because `docs/rules/input.md` does not exist yet — **that's expected**; it will be created in Task 10. Temporarily, one meta-test failing is acceptable across Tasks 4–9. If that test's filter-message output hides real regressions, run the suite with an explicit exclusion: `dotnet test --solution … --filter-method "!*Every_rule_appears_in_its_category_overview_page*"`. DocTool stubs `docs/rules/LX500.md`, updates schema/presets.

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Rules/Input/LX500_TextBoxWithoutInputScope.cs tests/XamlLint.Core.Tests/Rules/Input/LX500_TextBoxWithoutInputScopeTest.cs AnalyzerReleases.Unshipped.md docs/rules/LX500.md schema/
git -C D:/GitHub/jizc/xaml-lint commit -m "feat: add LX500 — TextBox lacks InputScope"
```

---

## Task 5: `NumericRangeHelpers` helper

Adds the shared literal-double parser used by LX501 and LX502. A minimal helper because both rules follow the same pattern: read `Minimum` + `Maximum`, treat literal-numeric values as comparable, treat markup-extension values (`{Binding Low}`, `{StaticResource MaxValue}`) as unknowns that skip the rule. Invariant-culture parse so a German-locale developer's `,`-decimal separator doesn't cause false positives or negatives.

**Files:**
- Create: `src/XamlLint.Core/Helpers/NumericRangeHelpers.cs`
- Create: `tests/XamlLint.Core.Tests/Helpers/NumericRangeHelpersTest.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Helpers/NumericRangeHelpersTest.cs`:

```csharp
using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class NumericRangeHelpersTest
{
    [Fact]
    public void TryReadLiteralDouble_returns_null_for_null_attribute()
    {
        NumericRangeHelpers.TryReadLiteralDouble(null).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_parses_integer_literal()
    {
        var attr = new XAttribute("Minimum", "5");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().Be(5.0);
    }

    [Fact]
    public void TryReadLiteralDouble_parses_decimal_literal_with_invariant_culture()
    {
        var attr = new XAttribute("Maximum", "3.14");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().Be(3.14);
    }

    [Fact]
    public void TryReadLiteralDouble_parses_negative_literal()
    {
        var attr = new XAttribute("Minimum", "-2.5");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().Be(-2.5);
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_markup_extension()
    {
        var attr = new XAttribute("Minimum", "{Binding Low}");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_non_numeric_literal()
    {
        var attr = new XAttribute("Minimum", "banana");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_empty_literal()
    {
        var attr = new XAttribute("Minimum", "");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_whitespace_literal()
    {
        var attr = new XAttribute("Minimum", "   ");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*NumericRangeHelpersTest*"
```

Expected: build error — `NumericRangeHelpers` does not exist.

- [ ] **Step 3: Implement the helper**

Create `src/XamlLint.Core/Helpers/NumericRangeHelpers.cs`:

```csharp
using System.Globalization;
using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Small utilities for the <c>Minimum</c>/<c>Maximum</c> range rules (LX501, LX502).
/// Only literal, invariant-culture-parseable values compare; anything else (a markup
/// extension, an empty string, a non-numeric literal) returns <c>null</c> so the rule
/// skips the pair rather than producing a false positive.
/// </summary>
public static class NumericRangeHelpers
{
    /// <summary>
    /// Parses an attribute's value as an invariant-culture <see cref="double"/>. Returns
    /// <c>null</c> when <paramref name="attribute"/> is <c>null</c>, empty/whitespace, a
    /// markup extension (<c>{Binding …}</c>, <c>{StaticResource …}</c>, …), or an unparseable
    /// literal.
    /// </summary>
    public static double? TryReadLiteralDouble(XAttribute? attribute)
    {
        if (attribute is null) return null;
        var value = attribute.Value;
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (MarkupExtensionHelpers.IsMarkupExtension(value)) return null;

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
```

- [ ] **Step 4: Run the tests**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*NumericRangeHelpersTest*"
```

Expected: all 8 helper tests pass.

- [ ] **Step 5: Run the full suite**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
```

Expected: no new regressions (the `Every_rule_appears_in_its_category_overview_page` meta-test is still waiting on `input.md` / `deprecated.md` — keep deferring).

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Helpers/NumericRangeHelpers.cs tests/XamlLint.Core.Tests/Helpers/NumericRangeHelpersTest.cs
git -C D:/GitHub/jizc/xaml-lint commit -m "feat(core): add NumericRangeHelpers for literal-double parsing"
```

---

## Task 6: LX501 — Slider Minimum > Maximum

Flags `<Slider>` where both `Minimum` and `Maximum` are literal numbers and `Minimum > Maximum`. If either attribute is a binding or absent (framework default wins), the rule stays quiet. Locates the diagnostic on the `Minimum` attribute — that's the value the author most commonly typed wrong when the default `Maximum` is 0 or 1.

**Files:**
- Create: `src/XamlLint.Core/Rules/Input/LX501_SliderMinimumGreaterThanMaximum.cs`
- Create: `tests/XamlLint.Core.Tests/Rules/Input/LX501_SliderMinimumGreaterThanMaximumTest.cs`
- Modify: `AnalyzerReleases.Unshipped.md` (add LX501 row)

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Rules/Input/LX501_SliderMinimumGreaterThanMaximumTest.cs`:

```csharp
using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX501_SliderMinimumGreaterThanMaximumTest
{
    [Fact]
    public void Minimum_greater_than_Maximum_on_Wpf_is_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Minimum="10"|] Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Minimum_greater_than_Maximum_on_Maui_is_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    [|Minimum="10"|] Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Minimum_greater_than_Maximum_on_WinUI3_is_not_flagged()
    {
        // Per spec §3.5, LX501's dialects are Wpf + Maui only — UWP/WinUI raise a runtime
        // exception on this state, so static analysis is redundant there.
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="10" Maximum="5" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Minimum_equals_Maximum_is_not_flagged()
    {
        // A single-valued range is legal (degenerate but not an error).
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="5" Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Minimum_less_than_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="0" Maximum="100" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Only_Minimum_attribute_present_is_not_flagged()
    {
        // Missing Maximum means the framework default (typically 1.0) applies — unknown at
        // lint time, so stay quiet.
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="10" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Bound_Minimum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="{Binding Low}" Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Bound_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="10" Maximum="{Binding High}" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Decimal_Minimum_greater_than_Maximum_is_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Minimum="2.5"|] Maximum="1.5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_Slider_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <ProgressBar xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                         Minimum="10" Maximum="5" />
            """,
            Dialect.Wpf);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX501_SliderMinimumGreaterThanMaximumTest*"
```

Expected: build error — `LX501_SliderMinimumGreaterThanMaximum` does not exist.

- [ ] **Step 3: Implement the rule**

Create `src/XamlLint.Core/Rules/Input/LX501_SliderMinimumGreaterThanMaximum.cs`:

```csharp
using System.Globalization;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX501",
    UpstreamId = "RXT330",
    Title = "Slider Minimum is greater than Maximum",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Wpf | Dialect.Maui,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX501.md")]
public sealed partial class LX501_SliderMinimumGreaterThanMaximum : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Slider") continue;

            var minAttr = element.Attribute("Minimum");
            var maxAttr = element.Attribute("Maximum");

            var min = NumericRangeHelpers.TryReadLiteralDouble(minAttr);
            var max = NumericRangeHelpers.TryReadLiteralDouble(maxAttr);
            if (min is null || max is null) continue;
            if (min.Value <= max.Value) continue;

            // minAttr is guaranteed non-null when min has a value (helper returns null for null input).
            var span = LocationHelpers.GetAttributeSpan(minAttr!, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"Slider Minimum=\"{min.Value.ToString(CultureInfo.InvariantCulture)}\" is greater than Maximum=\"{max.Value.ToString(CultureInfo.InvariantCulture)}\"; the range is empty.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
```

- [ ] **Step 4: Append the LX501 row to `AnalyzerReleases.Unshipped.md`**

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX201   | Bindings | Info     | Prefer x:Bind over Binding
LX301   | Naming   | Warning  | x:Uid should start with uppercase
LX500   | Input    | Info     | TextBox lacks InputScope
LX501   | Input    | Warning  | Slider Minimum is greater than Maximum
```

- [ ] **Step 5: Run the tests and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX501_SliderMinimumGreaterThanMaximumTest*"
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release
```

Expected: LX501 tests pass. `Every_rule_appears_in_its_category_overview_page` still waiting on Task 10. DocTool stubs `docs/rules/LX501.md`, updates schema/presets.

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Rules/Input/LX501_SliderMinimumGreaterThanMaximum.cs tests/XamlLint.Core.Tests/Rules/Input/LX501_SliderMinimumGreaterThanMaximumTest.cs AnalyzerReleases.Unshipped.md docs/rules/LX501.md schema/
git -C D:/GitHub/jizc/xaml-lint commit -m "feat: add LX501 — Slider Minimum is greater than Maximum"
```

---

## Task 7: LX502 — Stepper Minimum > Maximum

MAUI-only mirror of LX501 for the `<Stepper>` control. Same helper, same shape; differs only by element name (`Stepper`) and `Dialects = Dialect.Maui`.

**Files:**
- Create: `src/XamlLint.Core/Rules/Input/LX502_StepperMinimumGreaterThanMaximum.cs`
- Create: `tests/XamlLint.Core.Tests/Rules/Input/LX502_StepperMinimumGreaterThanMaximumTest.cs`
- Modify: `AnalyzerReleases.Unshipped.md` (add LX502 row)

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Rules/Input/LX502_StepperMinimumGreaterThanMaximumTest.cs`:

```csharp
using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX502_StepperMinimumGreaterThanMaximumTest
{
    [Fact]
    public void Minimum_greater_than_Maximum_on_Maui_is_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     [|Minimum="10"|] Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Minimum_greater_than_Maximum_on_Wpf_is_not_flagged()
    {
        // Stepper is a MAUI-only control; on WPF the element name means nothing framework-wise
        // and the rule's Dialects mask filters the dispatcher call out entirely.
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     Minimum="10" Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Minimum_less_than_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="0" Maximum="100" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Minimum_equals_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="5" Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Bound_Minimum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="{Binding Low}" Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Only_Minimum_attribute_present_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="10" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Non_Stepper_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    Minimum="10" Maximum="5" />
            """,
            Dialect.Maui);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX502_StepperMinimumGreaterThanMaximumTest*"
```

Expected: build error — `LX502_StepperMinimumGreaterThanMaximum` does not exist.

- [ ] **Step 3: Implement the rule**

Create `src/XamlLint.Core/Rules/Input/LX502_StepperMinimumGreaterThanMaximum.cs`:

```csharp
using System.Globalization;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX502",
    UpstreamId = "RXT335",
    Title = "Stepper Minimum is greater than Maximum",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX502.md")]
public sealed partial class LX502_StepperMinimumGreaterThanMaximum : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Stepper") continue;

            var minAttr = element.Attribute("Minimum");
            var maxAttr = element.Attribute("Maximum");

            var min = NumericRangeHelpers.TryReadLiteralDouble(minAttr);
            var max = NumericRangeHelpers.TryReadLiteralDouble(maxAttr);
            if (min is null || max is null) continue;
            if (min.Value <= max.Value) continue;

            var span = LocationHelpers.GetAttributeSpan(minAttr!, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"Stepper Minimum=\"{min.Value.ToString(CultureInfo.InvariantCulture)}\" is greater than Maximum=\"{max.Value.ToString(CultureInfo.InvariantCulture)}\"; the range is empty.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
```

- [ ] **Step 4: Append the LX502 row to `AnalyzerReleases.Unshipped.md`**

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX201   | Bindings | Info     | Prefer x:Bind over Binding
LX301   | Naming   | Warning  | x:Uid should start with uppercase
LX500   | Input    | Info     | TextBox lacks InputScope
LX501   | Input    | Warning  | Slider Minimum is greater than Maximum
LX502   | Input    | Warning  | Stepper Minimum is greater than Maximum
```

- [ ] **Step 5: Run the tests and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX502_StepperMinimumGreaterThanMaximumTest*"
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release
```

Expected: LX502 tests pass. `Every_rule_appears_in_its_category_overview_page` still waiting on Task 10. DocTool stubs `docs/rules/LX502.md`.

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Rules/Input/LX502_StepperMinimumGreaterThanMaximum.cs tests/XamlLint.Core.Tests/Rules/Input/LX502_StepperMinimumGreaterThanMaximumTest.cs AnalyzerReleases.Unshipped.md docs/rules/LX502.md schema/
git -C D:/GitHub/jizc/xaml-lint commit -m "feat: add LX502 — Stepper Minimum is greater than Maximum"
```

---

## Task 8: LX600 — MediaElement deprecated

Opens the `Deprecated` category (LX6xx). Flags `<MediaElement>` on UWP/WinUI 3 and suggests `<MediaPlayerElement>` (the modern replacement). WPF still ships `MediaElement` as the primary media API, so the rule's mask excludes it. Element-name filter only; no attribute inspection.

**Files:**
- Create: `src/XamlLint.Core/Rules/Deprecated/LX600_MediaElementDeprecated.cs`
- Create: `tests/XamlLint.Core.Tests/Rules/Deprecated/LX600_MediaElementDeprecatedTest.cs`
- Modify: `AnalyzerReleases.Unshipped.md` (add LX600 row)

- [ ] **Step 1: Write the failing tests**

Create `tests/XamlLint.Core.Tests/Rules/Deprecated/LX600_MediaElementDeprecatedTest.cs`:

```csharp
using XamlLint.Core.Rules.Deprecated;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Deprecated;

public sealed class LX600_MediaElementDeprecatedTest
{
    [Fact]
    public void MediaElement_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="video.mp4" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void MediaElement_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="video.mp4" />
            </Grid>
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void MediaElement_on_Wpf_is_not_flagged()
    {
        // WPF's MediaElement is current; MediaPlayerElement is UWP/WinUI only.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void MediaPlayerElement_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaPlayerElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_MediaElements_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="a.mp4" />
                <[|MediaElement|] Source="b.mp4" />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX600_MediaElementDeprecatedTest*"
```

Expected: build error — `LX600_MediaElementDeprecated` does not exist; the `Deprecated` namespace doesn't exist.

- [ ] **Step 3: Implement the rule**

Create `src/XamlLint.Core/Rules/Deprecated/LX600_MediaElementDeprecated.cs`:

```csharp
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Deprecated;

[XamlRule(
    Id = "LX600",
    UpstreamId = "RXT402",
    Title = "MediaElement is deprecated — use MediaPlayerElement",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX600.md")]
public sealed partial class LX600_MediaElementDeprecated : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "MediaElement") continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"MediaElement is deprecated on {context.Dialect}; use MediaPlayerElement instead.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
```

- [ ] **Step 4: Append the LX600 row to `AnalyzerReleases.Unshipped.md`**

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category   | Severity | Notes
--------|------------|----------|-------
LX201   | Bindings   | Info     | Prefer x:Bind over Binding
LX301   | Naming     | Warning  | x:Uid should start with uppercase
LX500   | Input      | Info     | TextBox lacks InputScope
LX501   | Input      | Warning  | Slider Minimum is greater than Maximum
LX502   | Input      | Warning  | Stepper Minimum is greater than Maximum
LX600   | Deprecated | Warning  | MediaElement is deprecated — use MediaPlayerElement
```

(Note: the Category column widens from 8 to 10 characters to fit `Deprecated`. The `Analyzer_release_category_column_matches_category_derivation` meta-test parses by `|`-split and ignores padding width, so widening the column is safe.)

- [ ] **Step 5: Run the tests and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*LX600_MediaElementDeprecatedTest*"
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release
```

Expected: LX600 tests pass. `Every_rule_appears_in_its_category_overview_page` still failing (now for 6 rules: LX201, LX301, LX500, LX501, LX502, LX600); will pass after Tasks 9 and 10. DocTool stubs `docs/rules/LX600.md`.

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add src/XamlLint.Core/Rules/Deprecated/LX600_MediaElementDeprecated.cs tests/XamlLint.Core.Tests/Rules/Deprecated/LX600_MediaElementDeprecatedTest.cs AnalyzerReleases.Unshipped.md docs/rules/LX600.md schema/
git -C D:/GitHub/jizc/xaml-lint commit -m "feat: add LX600 — MediaElement is deprecated"
```

---

## Task 9: Author per-rule docs

Replaces the DocTool-generated stubs at `docs/rules/LX201.md`, `LX301.md`, `LX500.md`, `LX501.md`, `LX502.md`, `LX600.md` with authored content following the 4-heading template (Cause / Rule description / How to fix violations / How to suppress violations) from spec §11.2. This task is docs-only; no code changes, no test changes.

**Files:**
- Modify: `docs/rules/LX201.md`
- Modify: `docs/rules/LX301.md`
- Modify: `docs/rules/LX500.md`
- Modify: `docs/rules/LX501.md`
- Modify: `docs/rules/LX502.md`
- Modify: `docs/rules/LX600.md`

- [ ] **Step 1: Author `docs/rules/LX201.md`**

Replace the stub contents entirely with:

````markdown
# LX201: Prefer x:Bind over Binding

<!-- Upstream: RXT170. -->

## Cause

An attribute value uses the `{Binding …}` markup extension on a UWP or WinUI 3 XAML file.
The rule is informational — both binding forms compile and run — but the project's target
dialect offers the compiled `{x:Bind …}` alternative.

## Rule description

On UWP and WinUI 3, `{x:Bind}` compiles to generated code that participates in view-model
type checking at build time and runs faster than runtime-reflected `{Binding}`. The
compile-time validation alone catches a large class of "silently wrong path" bugs that
`{Binding}` surfaces only in production. The rule flags any attribute whose markup
extension name is exactly `Binding`, including those with complex argument shapes such as
nested converters.

`{TemplateBinding …}` and `{x:Bind …}` are not flagged. Literal values and non-binding
extensions (`{StaticResource …}`, `{ThemeResource …}`, etc.) are out of scope.

```xaml
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- non-compliant on UWP/WinUI 3 -->
    <Button Content="{Binding Label}" />
</Grid>
```

```xaml
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- compliant: compiled binding, type-checked at build -->
    <Button Content="{x:Bind Label}" />
</Grid>
```

## How to fix violations

1. Set `x:DataType` on the enclosing view or template so `{x:Bind}` has a strong typing
   root (`<Page x:DataType="vm:MyViewModel" …>`).
2. Replace `{Binding Path}` with `{x:Bind Path}`. Remember that `{x:Bind}`'s default mode
   is `OneTime`; add `, Mode=OneWay` or `, Mode=TwoWay` to match the previous behavior when
   the UI needs to react to changes.
3. Rebuild and resolve any new compiler errors — these catch real path or typing bugs that
   were silent under `{Binding}`.

## How to suppress violations

For a single element:

```xaml
<!-- xaml-lint disable once LX201 -->
<Button Content="{Binding Label}" />
```

For a block:

```xaml
<!-- xaml-lint disable LX201 -->
<Button Content="{Binding First}" />
<Button Content="{Binding Second}" />
<!-- xaml-lint restore LX201 -->
```

For a whole file or project:

```json
{ "rules": { "LX201": "off" } }
```
````

- [ ] **Step 2: Author `docs/rules/LX301.md`**

Replace the stub contents entirely with:

````markdown
# LX301: x:Uid should start with uppercase

<!-- Upstream: RXT451. -->

## Cause

An `x:Uid` attribute in the XAML language namespace (2006 or 2009) has a value whose first
character is not an uppercase letter. The rule is scoped to UWP and WinUI 3, where `x:Uid`
is the resource-lookup key the resw runtime uses.

## Rule description

Resource keys on UWP/WinUI 3 follow PascalCase by convention: the `.resw` file's
`LoginButton.Content` entry maps to an element with `x:Uid="LoginButton"`. Lowercase or
underscore-prefixed keys work at runtime but break the convention that lets a reader
recognise resource-bound elements at a glance. The check only inspects the first
character; trailing characters and multi-word casing (`LoginButton` vs `Login_Button`) are
out of scope.

Only attributes whose name is `Uid` **and** whose namespace is the XAML 2006
(`http://schemas.microsoft.com/winfx/2006/xaml`) or 2009
(`http://schemas.microsoft.com/winfx/2009/xaml`) URI trigger the rule. Unprefixed
`Uid=` attributes are not XAML-language directives and are out of scope.

```xaml
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- non-compliant -->
    <Button x:Uid="loginButton" />
</Grid>
```

```xaml
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- compliant -->
    <Button x:Uid="LoginButton" />
</Grid>
```

## How to fix violations

Rename the `x:Uid` value to start with an uppercase letter. Update every matching key in
your `.resw` files (or localised equivalents) to use the new root. A project-wide
find-and-replace on `x:Uid="loginButton"` → `x:Uid="LoginButton"` followed by renaming the
resource keys is usually enough — no code-behind adjustments are required since `x:Uid`
does not generate a C# field.

## How to suppress violations

For a single element:

```xaml
<!-- xaml-lint disable once LX301 -->
<Button x:Uid="legacyKey" />
```

For a block:

```xaml
<!-- xaml-lint disable LX301 -->
<Button x:Uid="legacyKey1" />
<Button x:Uid="legacyKey2" />
<!-- xaml-lint restore LX301 -->
```

For a whole file or project:

```json
{ "rules": { "LX301": "off" } }
```
````

- [ ] **Step 3: Author `docs/rules/LX500.md`**

Replace the stub contents entirely with:

````markdown
# LX500: TextBox lacks InputScope

<!-- Upstream: RXT150. -->

## Cause

A UWP or WinUI 3 `<TextBox>` element is declared without an `InputScope` attribute.
`InputScope` is the hint the platform uses to select a soft keyboard layout (numeric,
URL, email, …) and to configure IME and auto-correction behavior.

## Rule description

The default `InputScope` yields a general-purpose keyboard, which is rarely the right thing
for fields that expect numbers, URLs, or email addresses. Missing the hint does not break
the app but consistently produces a slightly worse experience — users have to switch to
the numeric subpage themselves, and the system's auto-correct happily "corrects" a URL
into a sentence.

Any `InputScope` value — literal (`"Number"`, `"Url"`, `"EmailSmtpAddress"`, …) or bound
(`{Binding Scope}`) — is accepted. The rule does not try to validate the value; its job is
to flag the absence.

Only unprefixed `<TextBox>` elements are considered. Custom-namespaced types that happen
to end in `TextBox` are left alone.

```xaml
<StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <!-- non-compliant: defaults to the general-purpose keyboard -->
    <TextBox />
</StackPanel>
```

```xaml
<StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <!-- compliant: soft keyboard opens in numeric layout -->
    <TextBox InputScope="Number" />
</StackPanel>
```

## How to fix violations

Add an `InputScope` attribute with the most specific value that matches the field's
semantics. Common choices:

- `Number` — non-negative integers
- `NumberFullWidth` — numbers with optional decimal/sign
- `Url` — URLs and web addresses
- `EmailSmtpAddress` — email input
- `TelephoneNumber` — phone numbers
- `Password` — password entry (pairs with `PasswordBox` in practice)
- `Text` — explicit "general text" (silences the rule without changing behavior)

If no specific scope fits and the field genuinely is freeform prose, use `Text` explicitly
to document the decision.

## How to suppress violations

For a single element:

```xaml
<!-- xaml-lint disable once LX500 -->
<TextBox />
```

For a block:

```xaml
<!-- xaml-lint disable LX500 -->
<TextBox />
<TextBox />
<!-- xaml-lint restore LX500 -->
```

For a whole file or project:

```json
{ "rules": { "LX500": "off" } }
```
````

- [ ] **Step 4: Author `docs/rules/LX501.md`**

Replace the stub contents entirely with:

````markdown
# LX501: Slider Minimum is greater than Maximum

<!-- Upstream: RXT330. -->

## Cause

A `<Slider>` element sets both `Minimum` and `Maximum` as literal numeric values, and
`Minimum` is greater than `Maximum`. The control has an empty range and cannot produce any
value in a meaningful way.

## Rule description

WPF and .NET MAUI accept the inconsistent pair at parse time and clamp at runtime in
framework-specific ways — the result is a control that looks fine in the designer but
behaves surprisingly at runtime. UWP/WinUI 3 raise a runtime exception instead, so the
rule is WPF-and-MAUI-scoped.

Either `Minimum` or `Maximum` being a markup extension (binding, static resource, …)
suppresses the check: the actual value is unknown at lint time, so the rule prefers silence
to guessing. Missing either attribute also suppresses the check — the framework default
(typically 0 for `Minimum`, 1 or 100 for `Maximum`) applies.

```xaml
<!-- non-compliant: literal pair where min > max -->
<Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        Minimum="10" Maximum="5" />
```

```xaml
<!-- compliant: literal pair in ascending order -->
<Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        Minimum="0" Maximum="100" />
```

## How to fix violations

Swap the two values, or update whichever bound is wrong for the control's intended range.
If the values come from a view model, switch to `{Binding}` on one or both — a bound
`Minimum` or `Maximum` suppresses the rule, since the true value can't be checked at lint
time.

## How to suppress violations

For a single element:

```xaml
<!-- xaml-lint disable once LX501 -->
<Slider Minimum="10" Maximum="5" />
```

For a block:

```xaml
<!-- xaml-lint disable LX501 -->
<Slider Minimum="10" Maximum="5" />
<Slider Minimum="20" Maximum="15" />
<!-- xaml-lint restore LX501 -->
```

For a whole file or project:

```json
{ "rules": { "LX501": "off" } }
```
````

- [ ] **Step 5: Author `docs/rules/LX502.md`**

Replace the stub contents entirely with:

````markdown
# LX502: Stepper Minimum is greater than Maximum

<!-- Upstream: RXT335. -->

## Cause

A .NET MAUI `<Stepper>` element sets both `Minimum` and `Maximum` as literal numeric
values, and `Minimum` is greater than `Maximum`. The stepper has an empty range and cannot
produce any value in a meaningful way.

## Rule description

`Stepper` is the MAUI-specific numeric increment/decrement control. The semantics of an
inverted range are the same as [LX501](LX501.md): the control parses, but it does not work.
The rule is narrowed to MAUI only because `<Stepper>` is not a standard XAML element on
other dialects.

As with LX501, either value being a markup extension (`{Binding}`, `{StaticResource}`, …)
or missing suppresses the check.

```xaml
<!-- non-compliant -->
<Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
         Minimum="10" Maximum="5" />
```

```xaml
<!-- compliant -->
<Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
         Minimum="0" Maximum="10" />
```

## How to fix violations

Swap the two values, or update whichever bound is wrong for the stepper's intended range.
Using a bound value on either attribute also suppresses the rule.

## How to suppress violations

For a single element:

```xaml
<!-- xaml-lint disable once LX502 -->
<Stepper Minimum="10" Maximum="5" />
```

For a block:

```xaml
<!-- xaml-lint disable LX502 -->
<Stepper Minimum="10" Maximum="5" />
<Stepper Minimum="20" Maximum="15" />
<!-- xaml-lint restore LX502 -->
```

For a whole file or project:

```json
{ "rules": { "LX502": "off" } }
```
````

- [ ] **Step 6: Author `docs/rules/LX600.md`**

Replace the stub contents entirely with:

````markdown
# LX600: MediaElement is deprecated — use MediaPlayerElement

<!-- Upstream: RXT402. -->

## Cause

A `<MediaElement>` element appears in a UWP or WinUI 3 XAML file. Microsoft deprecated
`MediaElement` in favor of `MediaPlayerElement`, which wraps the newer `MediaPlayer` API
and offers better performance, clearer lifetime management, and access to modern playback
features.

## Rule description

`MediaPlayerElement` has been the recommended media host on UWP since the UWP 16299 SDK
(late 2017) and has been the only supported choice on WinUI 3 since release. The rule is
deliberately scoped to `Dialect.Uwp | Dialect.WinUI3` because WPF continues to ship
`MediaElement` as its primary media-playback control — the deprecation story is
UWP/WinUI-specific.

Only unprefixed `<MediaElement>` elements are considered. Custom-namespaced `MediaElement`
types are out of scope (they may be third-party wrappers with legitimate names).

```xaml
<!-- non-compliant on UWP/WinUI 3 -->
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <MediaElement Source="intro.mp4" AutoPlay="True" />
</Grid>
```

```xaml
<!-- compliant -->
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <MediaPlayerElement Source="intro.mp4" AutoPlay="True" AreTransportControlsEnabled="True" />
</Grid>
```

## How to fix violations

1. Replace the `<MediaElement>` tag with `<MediaPlayerElement>`. Most attributes (`Source`,
   `AutoPlay`, `IsMuted`, `Volume`) migrate directly.
2. `MediaElement.AreTransportControlsEnabled` is an implicit concept; on
   `MediaPlayerElement` set the attribute explicitly when you want the default playback UI
   (`AreTransportControlsEnabled="True"`).
3. Code-behind that interacted with `MediaElement.Play()` / `.Pause()` moves to
   `MediaPlayerElement.MediaPlayer.Play()` / `.Pause()` — the wrapped `MediaPlayer` is
   where state lives now.

## How to suppress violations

For a single element:

```xaml
<!-- xaml-lint disable once LX600 -->
<MediaElement Source="legacy.mp4" />
```

For a block:

```xaml
<!-- xaml-lint disable LX600 -->
<MediaElement Source="legacy1.mp4" />
<MediaElement Source="legacy2.mp4" />
<!-- xaml-lint restore LX600 -->
```

For a whole file or project:

```json
{ "rules": { "LX600": "off" } }
```
````

- [ ] **Step 7: Verify DocTool --check is clean**

```bash
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release -- --check
```

Expected: `no drift.`. Meta-tests that sniff for "generated stub" sentinel text in shipped rule docs (if any) should stay green.

- [ ] **Step 8: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add docs/rules/LX201.md docs/rules/LX301.md docs/rules/LX500.md docs/rules/LX501.md docs/rules/LX502.md docs/rules/LX600.md
git -C D:/GitHub/jizc/xaml-lint commit -m "docs: author LX201, LX301, LX500, LX501, LX502, LX600 rule pages"
```

---

## Task 10: Create `input.md` and `deprecated.md` category overview pages

Adds the two new category overview pages so the `Every_rule_appears_in_its_category_overview_page` meta-test passes for LX500/LX501/LX502/LX600. The existing category pages (`bindings.md`, `naming.md`) already cover LX201 and LX301 respectively, so only rows need to be appended there.

**Files:**
- Create: `docs/rules/input.md`
- Create: `docs/rules/deprecated.md`
- Modify: `docs/rules/bindings.md` (add LX201 row)
- Modify: `docs/rules/naming.md` (add LX301 row)

- [ ] **Step 1: Create `docs/rules/input.md`**

Create the file with the contents:

```markdown
# Input / controls (LX500–LX599)

Rules that check input controls for missing hints (keyboard layout, IME behavior) and
semantically inconsistent attribute pairs (out-of-order `Minimum`/`Maximum`). These rules
complement the layout and binding checks — an input control with an impossible range
parses fine but fails silently at runtime.

| ID | Title | Default |
|---|---|---|
| [LX500](LX500.md) | TextBox lacks InputScope | info |
| [LX501](LX501.md) | Slider Minimum is greater than Maximum | warning |
| [LX502](LX502.md) | Stepper Minimum is greater than Maximum | warning |
```

- [ ] **Step 2: Create `docs/rules/deprecated.md`**

Create the file with the contents:

```markdown
# Deprecated patterns (LX600–LX699)

Rules that flag XAML elements and attributes that were once idiomatic but have been
superseded by better replacements. Each rule points at the modern equivalent so the fix is
mechanical. The range is dialect-scoped: a pattern deprecated on UWP/WinUI 3 may still be
the primary API on WPF.

| ID | Title | Default |
|---|---|---|
| [LX600](LX600.md) | MediaElement is deprecated — use MediaPlayerElement | warning |
```

- [ ] **Step 3: Update `docs/rules/bindings.md` — add LX201 row**

The existing file has a single row for LX200. Append an LX201 row to the table so the full
contents become:

```markdown
# Bindings / data (LX200–LX299)

Rules that inspect data-binding expressions (`{Binding …}`, `{x:Bind …}`, `{TemplateBinding …}`).
These rules fire on attributes whose values are XAML markup extensions and examine the
extension's arguments — they do not run type analysis or verify data-context paths.

| ID | Title | Default |
|---|---|---|
| [LX200](LX200.md) | SelectedItem binding should be TwoWay | info |
| [LX201](LX201.md) | Prefer x:Bind over Binding | info |
```

- [ ] **Step 4: Update `docs/rules/naming.md` — add LX301 row**

The existing file has a single row for LX300. Append an LX301 row so the full contents
become:

```markdown
# Naming (LX300–LX399)

Rules that check identifier conventions on XAML attributes like `x:Name`, `x:Uid`, and
`x:Key`. These rules enforce project-wide consistency for names that are referenced from
code-behind, resource dictionaries, and animation storyboards.

| ID | Title | Default |
|---|---|---|
| [LX300](LX300.md) | x:Name should start with uppercase | warning |
| [LX301](LX301.md) | x:Uid should start with uppercase | warning |
```

- [ ] **Step 5: Verify meta-tests pass**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --filter-method "*Every_rule_appears_in_its_category_overview_page*"
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
```

Expected: both runs pass. The category-overview-link meta-test now finds every rule.

- [ ] **Step 6: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add docs/rules/input.md docs/rules/deprecated.md docs/rules/bindings.md docs/rules/naming.md
git -C D:/GitHub/jizc/xaml-lint commit -m "docs: add Input and Deprecated category overviews; add LX201/LX301 to existing pages"
```

---

## Task 11: RXT comparison + README + CHANGELOG updates for v0.4.0

Adds LX201/LX301/LX500/LX501/LX502/LX600 rows to the RXT comparison table, bumps the README status paragraph and platform-support matrix, and extends `CHANGELOG.md` with a `## [0.4.0]` section. These are all prose updates; nothing regenerated.

**Files:**
- Modify: `docs/comparison-with-rapid-xaml-toolkit.md`
- Modify: `README.md`
- Modify: `CHANGELOG.md`

- [ ] **Step 1: Add the six new rows to the RXT comparison table**

In `docs/comparison-with-rapid-xaml-toolkit.md`, the current table is ordered by `xaml-lint`
ID. Insert rows so the monotonic ordering is preserved:

- LX201 goes after `LX200`.
- LX301 goes after `LX300`.
- LX500, LX501, LX502 go after `LX400`.
- LX600 goes after LX502.

Concretely, after the existing `| LX200 | RXT160 | … |` line, insert:

```markdown
| LX201 | RXT170 | Prefer x:Bind over Binding. Scoped to UWP/WinUI 3 per upstream semantics — on those dialects `{x:Bind}` compiles to generated code and validates paths at build time. |
```

After the existing `| LX300 | RXT452 | … |` line, insert:

```markdown
| LX301 | RXT451 | x:Uid should start with uppercase. UWP/WinUI 3 only; `x:Uid` has no runtime meaning on WPF. Mirror of LX300 for `x:Uid`. |
```

After the existing `| LX400 | RXT200 | … |` line, insert:

```markdown
| LX500 | RXT150 | TextBox lacks InputScope. UWP/WinUI 3 only — `InputScope` is a platform-specific hint that does not exist on WPF. Any literal or bound value suppresses the check. |
| LX501 | RXT330 | Slider Minimum is greater than Maximum. WPF and MAUI only; UWP/WinUI raise a runtime exception on the same state, so static analysis is redundant there. Literal pair required — markup extensions on either attribute suppress the check. |
| LX502 | RXT335 | Stepper Minimum is greater than Maximum. MAUI-only control; same semantics as LX501. |
| LX600 | RXT402 | MediaElement deprecated — use MediaPlayerElement. UWP/WinUI 3 only; WPF continues to ship `MediaElement` as its primary media control. |
```

Also add behavior-difference notes. After the existing "LX400 vs RXT200" paragraph (at the
end of the `## Behavior differences` section), append:

```markdown
- **LX201 vs RXT170** — xaml-lint flags every `{Binding …}` attribute on UWP/WinUI 3, with
  no heuristic for "is this form likely convertible to `{x:Bind}`?". The intent is a noisy
  informational signal that Claude and human reviewers can triage case-by-case; projects
  mid-migration typically suppress at the file or glob level.
- **LX501/LX502 vs RXT330/RXT335** — xaml-lint requires both attributes to be literal
  numbers before firing. Upstream Rapid XAML Toolkit also flags the case when only one
  attribute is literal and the other is bound; we defer that until the false-positive rate
  on real projects is known.
```

- [ ] **Step 2: Bump the README status paragraph and platform-support matrix**

In `README.md`, find the status line that currently reads:

```markdown
v0.3.0 — Grid-layout rules shipped: [LX100](docs/rules/LX100.md) (Grid.Row without RowDefinition), [LX101](docs/rules/LX101.md) (Grid.Column without ColumnDefinition), [LX102](docs/rules/LX102.md) (Grid.RowSpan exceeds rows), and [LX103](docs/rules/LX103.md) (Grid.ColumnSpan exceeds columns), on top of v0.2.0's content rules (LX200, LX300, LX400) and v0.1.0's six tool/engine diagnostics (LX001–LX006). Full catalog at [docs/rules/](docs/rules/). See [CHANGELOG.md](CHANGELOG.md) for release history.
```

Replace with:

```markdown
v0.4.0 — Dialect-gated rules shipped: [LX201](docs/rules/LX201.md) (prefer x:Bind), [LX301](docs/rules/LX301.md) (x:Uid casing), [LX500](docs/rules/LX500.md) (TextBox InputScope), [LX501](docs/rules/LX501.md) (Slider Minimum > Maximum), [LX502](docs/rules/LX502.md) (Stepper Minimum > Maximum), and [LX600](docs/rules/LX600.md) (MediaElement deprecated), on top of v0.3.0's Grid-family rules (LX100–LX104), v0.2.0's content rules (LX200, LX300, LX400), and v0.1.0's six tool/engine diagnostics (LX001–LX006). Full catalog at [docs/rules/](docs/rules/). See [CHANGELOG.md](CHANGELOG.md) for release history.
```

Also update the platform-support table — WinUI 3, UWP, and .NET MAUI gain first-rule
coverage in M4. Replace the existing matrix:

```markdown
| Platform | Status |
| --- | --- |
| WPF | In progress |
| WinUI 3 | Planned |
| UWP | Planned |
| .NET MAUI | Planned |
| Avalonia | Planned |
| Uno Platform | Planned |
```

with:

```markdown
| Platform | Status |
| --- | --- |
| WPF | In progress |
| WinUI 3 | Partial (dialect-gated rules only) |
| UWP | Partial (dialect-gated rules only) |
| .NET MAUI | Partial (dialect-gated rules only) |
| Avalonia | Planned |
| Uno Platform | Planned |
```

- [ ] **Step 3: Add the v0.4.0 CHANGELOG section**

In `CHANGELOG.md`, below the `## [Unreleased]` marker and above `## [0.3.0] - 2026-04-18`, insert a new `[0.4.0]` section (use today's date — the currentDate in this session is `2026-04-18`, so the section header is `## [0.4.0] - 2026-04-18`; verify the real date when executing this task and adjust if different):

```markdown
## [0.4.0] - 2026-04-18

M4 — dialect-gated rules spanning UWP/WinUI 3, .NET MAUI, and WPF.

### Added

- [LX201](docs/rules/LX201.md) — Prefer x:Bind over Binding ([#5])
- [LX301](docs/rules/LX301.md) — x:Uid should start with uppercase ([#5])
- [LX500](docs/rules/LX500.md) — TextBox lacks InputScope ([#5])
- [LX501](docs/rules/LX501.md) — Slider Minimum is greater than Maximum ([#5])
- [LX502](docs/rules/LX502.md) — Stepper Minimum is greater than Maximum ([#5])
- [LX600](docs/rules/LX600.md) — MediaElement is deprecated — use MediaPlayerElement ([#5])
- Category overview pages: [input](docs/rules/input.md), [deprecated](docs/rules/deprecated.md) ([#5])
- `NumericRangeHelpers` in `XamlLint.Core` — literal-double parsing shared between LX501 and LX502 ([#5])
```

Also update the link collection at the bottom of the file. Replace:

```markdown
[Unreleased]: https://github.com/jizc/xaml-lint/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/jizc/xaml-lint/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/jizc/xaml-lint/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/jizc/xaml-lint/releases/tag/v0.1.0
[#2]: https://github.com/jizc/xaml-lint/pull/2
[#3]: https://github.com/jizc/xaml-lint/pull/3
[#4]: https://github.com/jizc/xaml-lint/pull/4
```

with:

```markdown
[Unreleased]: https://github.com/jizc/xaml-lint/compare/v0.4.0...HEAD
[0.4.0]: https://github.com/jizc/xaml-lint/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/jizc/xaml-lint/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/jizc/xaml-lint/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/jizc/xaml-lint/releases/tag/v0.1.0
[#2]: https://github.com/jizc/xaml-lint/pull/2
[#3]: https://github.com/jizc/xaml-lint/pull/3
[#4]: https://github.com/jizc/xaml-lint/pull/4
[#5]: https://github.com/jizc/xaml-lint/pull/5
```

(The PR number `#5` assumes the next M4 PR is number 5 — verify with `gh pr list --state all --limit 5` before committing. If the number has advanced, update every `[#5]` occurrence in the added `## [0.4.0]` section and the link collection to match.)

- [ ] **Step 4: Verify test suite and DocTool**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release -- --check
```

Expected: all tests pass; DocTool reports `no drift.`.

- [ ] **Step 5: Commit**

```bash
git -C D:/GitHub/jizc/xaml-lint add docs/comparison-with-rapid-xaml-toolkit.md README.md CHANGELOG.md
git -C D:/GitHub/jizc/xaml-lint commit -m "docs: update RXT comparison, README, and CHANGELOG for v0.4.0"
```

---

## Task 12: Open PR `M4: dialect-gated rules (v0.4.0)`

Opens the review PR against `main`. Expected commit tree on `m4-dialect-gated-rules` after Tasks 1–11: twelve commits — one per rule (six), one per helper (`NumericRangeHelpers`), one per docs task (per-rule + category overviews), one for RXT/README/CHANGELOG, one for the version bump.

**Files:**
- None (command-only task).

- [ ] **Step 1: Push the branch**

```bash
git -C D:/GitHub/jizc/xaml-lint push -u origin m4-dialect-gated-rules
```

Expected: push succeeds; branch tracked against `origin/m4-dialect-gated-rules`.

- [ ] **Step 2: Open the PR**

```bash
gh pr create --title "M4: dialect-gated rules (v0.4.0)" --body "$(cat <<'EOF'
## Summary

- Adds `LX201` (prefer x:Bind over Binding; UWP/WinUI 3, Info), `LX301` (x:Uid uppercase casing; UWP/WinUI 3, Warning), `LX500` (TextBox InputScope; UWP/WinUI 3, Info), `LX501` (Slider Minimum > Maximum; WPF/MAUI, Warning), `LX502` (Stepper Minimum > Maximum; MAUI only, Warning), `LX600` (MediaElement deprecated → MediaPlayerElement; UWP/WinUI 3, Warning).
- Introduces `NumericRangeHelpers` (literal-double parse skipping markup extensions) shared by LX501 and LX502.
- Adds `docs/rules/input.md` and `docs/rules/deprecated.md` category overview pages; extends `bindings.md` and `naming.md` with LX201/LX301 rows.
- Authors per-rule docs for all six new rules following the 4-heading template.
- Updates comparison-with-RXT with new rows and behavior-difference notes for the "literal-only" semantics of LX501/LX502 and the "flag-every-Binding" semantics of LX201.
- Bumps README status line and moves WinUI 3 / UWP / .NET MAUI to Partial in the platform-support matrix — M4 is the first milestone to exercise the dispatcher's dialect-mask gate.
- Graduation of the six `AnalyzerReleases.Unshipped.md` rows to `AnalyzerReleases.Shipped.md` follows in the release task after merge.

## Test plan

- [ ] `dotnet test --solution xaml-lint.slnx` green locally (~50+ new tests across the six rules plus the helper).
- [ ] `dotnet run --project src/XamlLint.DocTool -- --check` reports no drift.
- [ ] CI matrix (Windows / Ubuntu / macOS × net10) passes.
- [ ] Meta-tests pass: category column matches `XamlLintCategory.ForId`; every rule is linked from its category overview page; every rule has an authored (non-stub) doc file.
- [ ] Smoke-test dialect gating by running a fixture through the CLI with `--dialect wpf` and confirming LX201/LX301/LX500/LX600 emit nothing, then with `--dialect winui3` and confirming they emit where expected.
EOF
)"
```

Expected: a new PR is opened; the command prints its URL. Note the PR number for Task 13.

- [ ] **Step 3: Request review (optional)**

If the repository has a default reviewer set, skip. Otherwise assign yourself and await CI.

---

## Task 13: Graduate Unshipped → Shipped, bump plugin manifest, tag v0.4.0

After the PR from Task 12 is merged into `main`, graduate the six rows from `AnalyzerReleases.Unshipped.md` into a new `## Release 0.4.0` section in `AnalyzerReleases.Shipped.md`, empty the Unshipped file (keep the header skeleton for M5+), bump the plugin manifest to `0.4.0`, and push the `v0.4.0` tag. Same pattern M2/M3 used on graduation.

**Files:**
- Modify: `AnalyzerReleases.Shipped.md` (append `## Release 0.4.0` section)
- Modify: `AnalyzerReleases.Unshipped.md` (drain rule rows)
- Modify: `.claude-plugin/plugin.json` (bump to `0.4.0`)

- [ ] **Step 1: Switch to `main` and pull**

```bash
git -C D:/GitHub/jizc/xaml-lint checkout main
git -C D:/GitHub/jizc/xaml-lint pull origin main
```

Expected: `main` is up-to-date with the merged M4 PR.

- [ ] **Step 2: Append the 0.4.0 section to `AnalyzerReleases.Shipped.md`**

Append to the end of `AnalyzerReleases.Shipped.md` (after the existing `## Release 0.3.0` section):

```markdown

## Release 0.4.0

### New Rules

Rule ID | Category   | Severity | Notes
--------|------------|----------|-------
LX201   | Bindings   | Info     | Prefer x:Bind over Binding
LX301   | Naming     | Warning  | x:Uid should start with uppercase
LX500   | Input      | Info     | TextBox lacks InputScope
LX501   | Input      | Warning  | Slider Minimum is greater than Maximum
LX502   | Input      | Warning  | Stepper Minimum is greater than Maximum
LX600   | Deprecated | Warning  | MediaElement is deprecated — use MediaPlayerElement
```

- [ ] **Step 3: Drain `AnalyzerReleases.Unshipped.md`**

Replace the file's contents with the empty skeleton (keep the header for M5's first rule):

```markdown
; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
```

- [ ] **Step 4: Bump the plugin manifest version**

In `.claude-plugin/plugin.json`, change `"version": "0.3.0"` to `"version": "0.4.0"`.

- [ ] **Step 5: Verify**

```bash
dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx
dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release -- --check
```

Expected: all tests pass (the `AnalyzerReleases` meta-test now sees LX201/LX301/LX500/LX501/LX502/LX600 in Shipped and zero rule rows in Unshipped); DocTool reports no drift.

- [ ] **Step 6: Commit the graduation**

```bash
git -C D:/GitHub/jizc/xaml-lint add AnalyzerReleases.Shipped.md AnalyzerReleases.Unshipped.md .claude-plugin/plugin.json
git -C D:/GitHub/jizc/xaml-lint commit -m "Graduate LX201, LX301, LX500-LX502, LX600 to AnalyzerReleases.Shipped.md for v0.4.0"
git -C D:/GitHub/jizc/xaml-lint push origin main
```

- [ ] **Step 7: Tag and push**

```bash
git -C D:/GitHub/jizc/xaml-lint tag -a v0.4.0 -m "v0.4.0 — dialect-gated rules: LX201, LX301, LX500, LX501, LX502, LX600"
git -C D:/GitHub/jizc/xaml-lint push origin v0.4.0
```

Expected: `v0.4.0` appears in `git tag --list --sort=-v:refname` after `v0.3.0`, `v0.2.0`, `v0.1.0`.

- [ ] **Step 8: Verify the NuGet package version**

```bash
git -C D:/GitHub/jizc/xaml-lint checkout v0.4.0
dotnet pack D:/GitHub/jizc/xaml-lint/src/XamlLint.Cli --configuration Release --output /tmp/xaml-lint-pack
ls /tmp/xaml-lint-pack
git -C D:/GitHub/jizc/xaml-lint checkout main
```

Expected: a file named `XamlLint.Cli.0.4.0.nupkg` (no pre-release suffix) in the output directory. This confirms `Nerdbank.GitVersioning`'s `publicReleaseRefSpec` recognises the tag.

---

## Self-review

**Spec coverage (design §10 M4 requirements):**

- `LX201` prefer x:Bind — Task 2 ✔
- `LX301` x:Uid casing — Task 3 ✔
- `LX500` TextBox InputScope — Task 4 ✔
- `LX501` Slider min/max — Task 6 ✔
- `LX502` Stepper min/max — Task 7 ✔
- `LX600` MediaElement deprecated — Task 8 ✔
- Exercises dialect-gated rule execution across UWP/WinUI 3/MAUI — every rule narrows its `Dialects` mask; tests at Tasks 2, 3, 4, 6, 7, 8 each assert both the firing and the non-firing dialect path, routing through `RuleDispatcher`'s `(meta.Dialects & document.Dialect) == 0` gate ✔
- Category overview pages `input.md`, `deprecated.md` — Task 10 ✔
- Graduation on tag — Task 13 ✔
- Tag `v0.4.0` — Task 13 ✔

**Severity defaults match spec §3.5:**

- LX201 Info ✔ — Task 2 attribute `DefaultSeverity = Severity.Info`
- LX301 Warning ✔ — Task 3
- LX500 Info ✔ — Task 4
- LX501 Warning ✔ — Task 6
- LX502 Warning ✔ — Task 7
- LX600 Warning ✔ — Task 8

**Dialect masks match spec §3.5:**

- LX201 `Dialect.Uwp | Dialect.WinUI3` ✔ — spec row says "Uwp, WinUI3"
- LX301 `Dialect.Uwp | Dialect.WinUI3` ✔
- LX500 `Dialect.Uwp | Dialect.WinUI3` ✔
- LX501 `Dialect.Wpf | Dialect.Maui` ✔
- LX502 `Dialect.Maui` ✔
- LX600 `Dialect.Uwp | Dialect.WinUI3` ✔

**Upstream IDs match spec §3.5 catalog table:**

- LX201 `RXT170` ✔
- LX301 `RXT451` ✔
- LX500 `RXT150` ✔
- LX501 `RXT330` ✔
- LX502 `RXT335` ✔
- LX600 `RXT402` ✔

**Placeholder scan:** No "TBD", "TODO", "fill in details", or "similar to Task N" — each task has concrete file paths, exact edit regions, actual code, and copy-pasteable commands.

**Type consistency:**

- `NumericRangeHelpers.TryReadLiteralDouble(XAttribute?)` (Task 5) is called in LX501 (Task 6) and LX502 (Task 7) with a nullable `XAttribute` and treated as returning `double?`. Signature and nullability match across producer and consumers.
- Every rule uses `LocationHelpers.GetAttributeSpan(XAttribute, ReadOnlyMemory<char>)` or `LocationHelpers.GetElementNameSpan(XElement)` — both signatures declared in M1/M3 and unchanged in M4.
- Every `[XamlRule(...)]` attribute follows the same property ordering (`Id`, `UpstreamId`, `Title`, `DefaultSeverity`, `Dialects`, `HelpUri`) used by LX200/LX300/LX400/LX100–LX104.
- `Metadata.Id`, `Metadata.DefaultSeverity`, `Metadata.HelpUri` are source-generated (declared by M1); every rule references them identically.
- `XamlDiagnosticVerifier<TRule>.Analyze(source, Dialect)` — the `Dialect` overload exists since M1 and is used throughout Tasks 2/3/4/6/7/8 without any signature change.
- `XamlLintCategory.ForId("LX201" | "LX301" | "LX500" | "LX501" | "LX502" | "LX600")` returns `Bindings` / `Naming` / `Input` / `Input` / `Input` / `Deprecated`. Every `AnalyzerReleases.Unshipped.md` row uses the matching category name, keeping `Analyzer_release_category_column_matches_category_derivation` green.
- Rule class names match their filenames (Tasks 2/3/4/6/7/8): `LX201_PreferXBind`, `LX301_XUidCasing`, `LX500_TextBoxWithoutInputScope`, `LX501_SliderMinimumGreaterThanMaximum`, `LX502_StepperMinimumGreaterThanMaximum`, `LX600_MediaElementDeprecated`.

**Known non-items (intentionally out of scope):**

- `Dialects` mask validation at rule-declaration time (reject `Dialect.None` at compile time via the source generator). The meta-test `Every_rule_has_non_zero_dialects` already catches this at test time — duplicating it as a generator diagnostic is not in scope.
- Richer `InputScope` value checking (e.g., warn when a `Number` scope is used on a field whose `Text`-binding target is typed `string`). Requires C# type analysis; deferred to v2's LSP work (spec §12).
- Static detection of "Binding is obviously x:Bind-convertible" to make LX201 less noisy (requires parsing `DataContext` / `x:DataType` propagation — hard). Deferred.
- Slider `Value` inside the flagged range (e.g., `Value="50"` when `Minimum="0" Maximum="10"`). Separate concern from LX501 and a candidate future rule.
- Auto-fix. LX201 (swap `Binding` → `x:Bind`) is the most plausibly fixable, but mode semantics differ (`Binding` OneWay → `x:Bind` OneTime) so a naive rename would change behavior; deferred per spec §12.
- Benchmark test. Spec §8.2 item 6 mentions a 50ms p95 budget but deferred to future work; M4 does not add benchmarks.

---

## Exit criteria

- `dotnet build D:/GitHub/jizc/xaml-lint/xaml-lint.slnx --configuration Release` — 0 warnings, 0 errors.
- `dotnet test --solution D:/GitHub/jizc/xaml-lint/xaml-lint.slnx` — every project green, including the 8 new `NumericRangeHelpersTest` cases and the ~50 rule-unit tests across the six new rules.
- `dotnet run --project D:/GitHub/jizc/xaml-lint/src/XamlLint.DocTool --configuration Release -- --check` — exit code 0, no drift.
- `git -C D:/GitHub/jizc/xaml-lint log --oneline main..m4-dialect-gated-rules` — shows one `feat/chore/docs` commit per task (Tasks 1–11). Review-driven `fix(core):` commits that land between rule commits are expected and should be preserved as audit trail.
- PR `M4: dialect-gated rules (v0.4.0)` merged; `v0.4.0` tag pushed; `AnalyzerReleases.Shipped.md` contains the new `## Release 0.4.0` section; `AnalyzerReleases.Unshipped.md` is empty of rows; plugin manifest reads `"version": "0.4.0"`.
