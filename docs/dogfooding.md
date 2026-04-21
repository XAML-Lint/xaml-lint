# Dogfooding regimen

Real-world XAML surfaces bugs the synthetic test suite can't: wrong-dialect gating,
over-narrow rule scopes, unexpected `{Binding}` forms, unusual attached-property
patterns. This doc is the checklist for running `xaml-lint` against a curated corpus
of open-source apps and samples — one repo per supported dialect — before shipping
a new rule, a dialect-scope change, or a rule-behavior tweak.

The process is manual today. Cadence is "when you touch rules," not "on every PR."

## When to run

Run the regimen against the affected dialect(s) whenever you:

- Add a new rule.
- Widen or narrow a rule's `Dialects` mask.
- Change a rule's detection logic (new attributes matched, new suppression shape).
- Tune a preset (`xaml-lint:recommended` / `xaml-lint:strict`) severity map.

Rules of thumb:

- If the change is a bug fix in a single dialect's logic, it's usually enough to
  dogfood just that dialect.
- If the change touches rule scope across dialects (most PRs in the Input,
  Deprecated, and Accessibility categories), run every dialect in the table below
  — the whole point is to catch "we said this applied to X, Y, Z and it didn't."
- For pure tooling changes (CLI flags, config plumbing, output formats), the
  synthetic test suite is sufficient; the dogfood corpus isn't needed.

## Corpus

Multiple repos per dialect, chosen for size, activity, and representativeness of
real production code. For any given rule change, running the first row per
dialect is usually enough; the supplementary rows exist to widen surface area
when a change affects rules that are likely to interact with component-library
patterns (heavy `ControlTemplate` / `Style` use, custom renderers, xmlns
conventions).

| Dialect    | Repo                                                                                                             | Why                                                                              |
|------------|------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------|
| WPF        | [microsoft/WPF-Samples](https://github.com/microsoft/WPF-Samples)                                                | Microsoft's own WPF sample set; broad feature coverage.                          |
| WPF        | [MaterialDesignInXAML/MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) | Popular WPF styling toolkit; heavy `ControlTemplate` / `Style` surface.    |
| WinUI 3    | [microsoft/WindowsAppSDK-Samples](https://github.com/microsoft/WindowsAppSDK-Samples)                            | Official WinAppSDK / WinUI 3 samples from Microsoft.                             |
| WinUI 3    | [files-community/Files](https://github.com/files-community/Files)                                                | Large, actively-developed WinUI 3 application — exercises realistic bindings.    |
| WinUI 3    | [CommunityToolkit/Windows](https://github.com/CommunityToolkit/Windows)                                          | Successor to Windows Community Toolkit; WinUI 3 component library.               |
| UWP        | [microsoft/Windows-universal-samples](https://github.com/microsoft/Windows-universal-samples)                    | Microsoft's full UWP sample set; the only remaining large UWP corpus.            |
| UWP        | [CommunityToolkit/MVVM-Samples](https://github.com/CommunityToolkit/MVVM-Samples) (`MvvmSampleUwp` subproject)   | Small MVVM demo; covers a second UWP surface.                                    |
| MAUI       | [dotnet/maui-samples](https://github.com/dotnet/maui-samples)                                                    | .NET team's official MAUI samples across every platform head.                    |
| MAUI       | [CommunityToolkit/Maui](https://github.com/CommunityToolkit/Maui)                                                | CT.Maui controls + samples; large real-world MAUI surface.                       |
| MAUI       | [CommunityToolkit/MVVM-Samples](https://github.com/CommunityToolkit/MVVM-Samples) (`MvvmSampleMAUI` subproject)  | Small MVVM demo; first repo where LX402 fires.                                   |
| Avalonia   | [AvaloniaUI/Avalonia.Samples](https://github.com/AvaloniaUI/Avalonia.Samples)                                    | Official Avalonia sample set.                                                    |
| Avalonia   | [WalletWasabi/WalletWasabi](https://github.com/WalletWasabi/WalletWasabi)                                        | Production Avalonia wallet app; large real-world surface.                        |
| Uno        | [unoplatform/Uno.Samples](https://github.com/unoplatform/Uno.Samples)                                            | Uno team's sample set; exercises the WinUI-compatible XAML surface on every head. |
| Uno        | [unoplatform/Uno.Gallery](https://github.com/unoplatform/Uno.Gallery)                                            | Control-showcase gallery; complements Uno.Samples with curated UX pages.         |

Clone each repo once, somewhere on your machine. The commands below assume you
`cd` into each repo before invoking `xaml-lint`.

The `CommunityToolkit/MVVM-Samples` repo has sibling subprojects for MAUI, UWP,
and Xamarin.Forms (`MvvmSampleXF`). Only the MAUI and UWP subprojects are
linted — Xamarin.Forms is not a supported dialect.

## How to run

First build and locate the CLI (or install the published tool):

```bash
# From the xaml-lint repo
dotnet publish --framework net10.0 -c Release src/XamlLint.Cli -o bin/cli
# Then invoke as ./bin/cli/xaml-lint.exe (or add to PATH)
```

Then, from inside each sample repo, run:

```bash
# WPF
cd path/to/WPF-Samples
xaml-lint lint --dialect wpf --format compact-json . -o /tmp/xl-wpf-samples.json

cd path/to/MaterialDesignInXamlToolkit
xaml-lint lint --dialect wpf --format compact-json . -o /tmp/xl-mdix.json

# WinUI 3
cd path/to/WindowsAppSDK-Samples
xaml-lint lint --dialect winui3 --format compact-json . -o /tmp/xl-wasdk.json

cd path/to/Files
xaml-lint lint --dialect winui3 --format compact-json . -o /tmp/xl-files.json

cd path/to/CommunityToolkit/Windows
xaml-lint lint --dialect winui3 --format compact-json . -o /tmp/xl-ct-windows.json

# UWP
cd path/to/Windows-universal-samples
xaml-lint lint --dialect uwp --format compact-json . -o /tmp/xl-uwp-samples.json

cd path/to/MVVM-Samples/samples/MvvmSampleUwp
xaml-lint lint --dialect uwp --format compact-json . -o /tmp/xl-ct-mvvm-uwp.json

# MAUI
cd path/to/maui-samples
xaml-lint lint --dialect maui --format compact-json . -o /tmp/xl-maui-samples.json

cd path/to/CommunityToolkit/Maui
xaml-lint lint --dialect maui --format compact-json . -o /tmp/xl-ct-maui.json

cd path/to/MVVM-Samples/samples/MvvmSampleMAUI
xaml-lint lint --dialect maui --format compact-json . -o /tmp/xl-ct-mvvm-maui.json

# Uno
cd path/to/Uno.Samples
xaml-lint lint --dialect uno --format compact-json . -o /tmp/xl-uno-samples.json

cd path/to/Uno.Gallery
xaml-lint lint --dialect uno --format compact-json . -o /tmp/xl-uno-gallery.json
```

Avalonia entries (`AvaloniaUI/Avalonia.Samples`, `WalletWasabi/WalletWasabi`)
are not yet runnable — both repos use `.axaml` and the tool currently rejects
that extension. See [issue #11](https://github.com/XAML-Lint/xaml-lint/issues/11).

Notes:

- Pass the directory as a positional argument (`.`), not a glob (`**/*.xaml`).
  Bash/zsh eat the glob before the tool sees it. The tool's built-in directory
  enumeration recurses for you.
- `compact-json` is the machine-readable envelope. For eyeballing, drop `--format`
  and pipe to less — the default TTY format is colored and per-file grouped.
- Do **not** pass `--verbosity quiet` when you want to see diagnostics:
  `quiet` suppresses non-error output from the envelope.

### Triaging a single diagnostic group

To zero in on one rule across the whole corpus:

```bash
xaml-lint lint --dialect uwp --only LX600 --format pretty . | less
```

## Interpreting results

The goal of each run is to ask two questions:

1. **Did any new diagnostics appear that shouldn't have?** If you widened a rule to
   a new dialect and it now fires on authored markup that is idiomatic and
   correct on that dialect, the widening is wrong. Either narrow the scope back
   or tighten the detection.
2. **Did any expected diagnostics disappear?** If you re-scoped a rule and it
   stopped firing on a dialect where it still applies, the new scope is wrong.

The workflow is a three-step diff:

1. Check out `main`, run the relevant dialect, save `before.json`.
2. Check out your branch, run the same dialect, save `after.json`.
3. Diff: `diff <(jq -S .results before.json) <(jq -S .results after.json)`. Review
   every delta; each one is either a bug your change caught, a bug your change
   introduced, or an intentional scope change you should be able to explain.

For the "widening LX600 to Uno" change in PR #10, for example, step 2 on
`Uno.Samples` was expected to produce new LX600 hits wherever that corpus uses
`<MediaElement>`. If step 2 produced zero new hits, that would indicate the
dialect gate wasn't wired up correctly.

## Known limitations

- **Avalonia `.axaml` files are not linted today.** The tool's `--force` flag
  lets `.axaml` files through file enumeration, but the per-file pipeline still
  treats them as non-XAML (emits `LX005` and skips). Until that is fixed, both
  Avalonia dogfood rows in the baseline snapshot below show zero diagnostics —
  a tooling artifact, not evidence the corpus is clean. Tracked in
  [issue #11](https://github.com/XAML-Lint/xaml-lint/issues/11); re-baseline
  both Avalonia rows once it ships.
- **LX702 can't see WPF reverse-labeling via `<Label Target="{x:Reference …}"/>`.**
  The canonical WPF pattern for associating a label with an input is to author the
  `Label` with a `Target` attribute that references the input's `x:Name`; at runtime
  WPF wires the automation peer so screen readers announce the label as the input's
  name. LX702 only inspects attributes on the `TextBox` itself (looking for
  `AutomationProperties.Name`/`Header`/`LabeledBy`), so a `TextBox` labeled this way
  looks unlabeled to the rule. No observed false positives in the current WPF-Samples
  corpus — none of the flagged `TextBox` elements sit in files that use `Label.Target`
  — but the gap is latent and would surface on a WPF codebase that follows the
  reverse-labeling convention consistently. Tracked informally here until it starts
  producing real-world noise; fixing it properly requires scanning the document for
  `Label` elements whose `Target` references resolve to the current `TextBox`'s name
  scope, which the existing `XamlNameIndex` infrastructure can do.
- **LX100 / LX101 can't see runtime-registered layout managers.** MAUI lets
  callers replace `GridLayoutManager` at runtime via an `ILayoutManagerFactory`,
  and the replacement can auto-add rows or columns based on attached-property
  values from child views. A `<Grid>` with no `RowDefinitions` but children at
  `Grid.Row="3"` is legitimate markup under such a factory, even though LX100
  will flag it. The canonical example in the corpus is
  `dotnet/maui-samples/…/CustomizedGridPage.xaml`, which exists to demonstrate
  exactly that pattern. Static analysis can't see the runtime factory
  registration, so these stay as expected hits in the baseline; suppress
  locally with `<!-- xaml-lint disable once LX100 -->` if your project uses a
  custom factory.
- **Sample repos drift.** The upstream sample repos get new content all the
  time, so absolute diagnostic counts will move even when `xaml-lint` doesn't
  change. The snapshot below is pinned to specific commit hashes; if you're
  checking for drift, either update to those commits or re-baseline `main`
  yourself before comparing.

## Baseline snapshot

Captured on 2026-04-21 from `xaml-lint` `main @ 9b8c875` against the commits
listed. Numbers are total diagnostics per repo.

| Dialect  | Repo                                             | Commit       | Total | By severity              | Top rule(s)                                      |
|----------|--------------------------------------------------|--------------|------:|--------------------------|--------------------------------------------------|
| WPF      | microsoft/WPF-Samples                            | `a121d7d9`   |    26 | 1 error, 19 warn, 6 info | LX101 (14), LX200 (6), LX100 (2), LX001 (1)      |
| WPF      | MaterialDesignInXAML/MaterialDesignInXamlToolkit | `70516d3b`   |    39 | 3 warn, 36 info          | LX200 (36), LX102 (2), LX100 (1)                 |
| WinUI 3  | microsoft/WindowsAppSDK-Samples                  | `9f033250`   |   222 | 21 warn, 201 info        | LX201 (144), LX500 (57), LX100 (15)              |
| WinUI 3  | files-community/Files                            | `fbc0a0b36`  |   225 | 19 warn, 206 info        | LX201 (166), LX500 (40), LX100 (11)              |
| WinUI 3  | CommunityToolkit/Windows                         | `b1d8231`    |   215 | 18 warn, 197 info        | LX201 (162), LX500 (35), LX100 (7)               |
| UWP      | microsoft/Windows-universal-samples              | `082195895`  |  1166 | 129 warn, 1037 info      | LX500 (534), LX201 (501), LX600 (47), LX100 (48) |
| UWP      | CommunityToolkit/MVVM-Samples/MvvmSampleUwp      | `7d67102`    |     4 | 1 warn, 3 info           | LX500 (2), LX103 (1), LX201 (1)                  |
| MAUI     | dotnet/maui-samples                              | `8b53f57a`   |   272 | 42 warn, 230 info        | LX503 (204), LX200 (26), LX101 (14), LX504 (12)  |
| MAUI     | CommunityToolkit/Maui                            | `bfc511e5`   |    81 | 29 warn, 52 info         | LX503 (43), LX101 (14), LX100 (13), LX200 (9)    |
| MAUI     | CommunityToolkit/MVVM-Samples/MvvmSampleMAUI     | `7d67102`    |     2 | 1 warn, 1 info           | LX402 (1), LX503 (1)                             |
| Avalonia | AvaloniaUI/Avalonia.Samples                      | `8956dbf`    |     0 | —                        | —                                                |
| Avalonia | WalletWasabi/WalletWasabi                        | `cc0e9c0291` |     0 | —                        | —                                                |
| Uno      | unoplatform/Uno.Samples                          | `1d9ea60a`   |  1334 | 43 warn, 1291 info       | LX201 (1185), LX500 (101), LX100 (14)            |
| Uno      | unoplatform/Uno.Gallery                          | `54a08b15`   |   613 | 1 warn, 612 info         | LX201 (540), LX500 (72), LX102 (1)               |

The one `error` in WPF-Samples is an LX001 parse failure on
`Documents/Fixed Documents/DocumentStructure/content/fixedpage1_structure.xaml` —
the file is a fragment with a leading `<` inside an attribute value, not a
well-formed XAML document. Treat it as a known-bad file in the corpus, not a
regression. The `0` on both Avalonia rows is a tooling artifact (see "Known
limitations"), not a clean corpus. The single LX402 hit on the MAUI MVVM sample
is the first time that rule has fired against the corpus — `Source="headerBg"`
in `FlyoutHeader.xaml:7` violates Android drawable naming (uppercase `B`), which
the rule catches correctly.

## Adding a repo

If you find a better sample set for a dialect — bigger, more idiomatic, more
actively maintained — open a PR that:

1. Adds the repo to the corpus table above.
2. Adds a baseline row to the snapshot with commit hash, total, and top rules.
3. Keeps or retires the existing entry depending on whether it still adds
   signal the new one doesn't.
