using System.Text.RegularExpressions;

namespace XamlLint.Core.Tests.Meta;

public sealed class CatalogMetaTest
{
    private static readonly IReadOnlyList<RuleMetadata> Rules =
        GeneratedRuleCatalog.Rules.Select(r => r.Metadata).ToList();

    [Fact]
    public void All_rule_ids_are_unique()
    {
        var dupes = Rules.GroupBy(m => m.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        dupes.Should().BeEmpty();
    }

    [Fact]
    public void Every_rule_has_non_zero_dialects()
    {
        foreach (var m in Rules)
            m.Dialects.Should().NotBe(Dialect.None, $"{m.Id} must declare at least one dialect");
    }

    [Fact]
    public void Every_rule_has_help_uri_matching_pattern()
    {
        var pattern = new Regex(@"^https://github\.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX\d{3}\.md$");
        foreach (var m in Rules)
            pattern.IsMatch(m.HelpUri).Should().BeTrue($"{m.Id} HelpUri must match expected pattern; got '{m.HelpUri}'");
    }

    [Fact]
    public void Upstream_id_when_present_matches_rxt_pattern()
    {
        var pattern = new Regex(@"^RXT\d+$");
        foreach (var m in Rules.Where(r => r.UpstreamId is not null))
            pattern.IsMatch(m.UpstreamId!).Should().BeTrue($"{m.Id} UpstreamId '{m.UpstreamId}' must match RXT\\d+");
    }

    [Fact]
    public void Every_rule_id_appears_in_analyzer_releases()
    {
        var repoRoot = FindRepoRoot();
        var shipped = File.ReadAllText(Path.Combine(repoRoot, "AnalyzerReleases.Shipped.md"));
        var unshipped = File.ReadAllText(Path.Combine(repoRoot, "AnalyzerReleases.Unshipped.md"));
        var combined = shipped + "\n" + unshipped;

        foreach (var m in Rules)
            combined.Should().Contain(m.Id, $"{m.Id} must appear in Shipped.md or Unshipped.md");
    }

    [Fact]
    public void Every_rule_has_a_docs_file()
    {
        var repoRoot = FindRepoRoot();
        foreach (var m in Rules)
        {
            var path = Path.Combine(repoRoot, "docs", "rules", $"{m.Id}.md");
            File.Exists(path).Should().BeTrue($"docs/rules/{m.Id}.md must exist (run DocTool)");
        }
    }

    [Fact]
    public void Rule_class_filename_matches_id()
    {
        foreach (var rule in GeneratedRuleCatalog.Rules)
        {
            var id = rule.Metadata.Id;
            var typeName = rule.GetType().Name;
            typeName.Should().StartWith(id + "_",
                $"rule class for {id} must be named '{id}_Something' (got '{typeName}')");
        }
    }

    [Fact]
    public void Deprecated_rules_have_replacement_or_justification()
    {
        // No deprecated rules in M1 — placeholder future-proofing.
        foreach (var m in Rules.Where(r => r.Deprecated))
            m.ReplacedBy.Should().NotBeNullOrEmpty($"deprecated {m.Id} must set ReplacedBy");
    }

    [Fact]
    public void Analyzer_release_category_column_matches_category_derivation()
    {
        var repoRoot = FindRepoRoot();
        var shipped = File.ReadAllText(Path.Combine(repoRoot, "AnalyzerReleases.Shipped.md"));
        var unshipped = File.ReadAllText(Path.Combine(repoRoot, "AnalyzerReleases.Unshipped.md"));
        var combined = shipped + "\n" + unshipped;

        // Match rows like:   LX300   | Naming   | Warning  | Description
        // The first two pipe-delimited cells are the ID and the category.
        var row = new System.Text.RegularExpressions.Regex(
            @"^(?<id>LX\d{3})\s*\|\s*(?<category>\w+)\s*\|",
            System.Text.RegularExpressions.RegexOptions.Multiline);

        foreach (System.Text.RegularExpressions.Match m in row.Matches(combined))
        {
            var id = m.Groups["id"].Value;
            var writtenCategory = m.Groups["category"].Value;
            var expected = XamlLintCategoryNames.NameOf(XamlLintCategoryExtensions.ForId(id));
            writtenCategory.Should().Be(expected,
                $"AnalyzerReleases row for {id} must list category '{expected}', got '{writtenCategory}'");
        }
    }

    [Fact]
    public void Every_rule_appears_in_its_category_overview_page()
    {
        var repoRoot = FindRepoRoot();
        foreach (var m in Rules)
        {
            var category = XamlLintCategoryExtensions.ForId(m.Id);
            var overviewFile = Path.Combine(
                repoRoot, "docs", "rules",
                XamlLintCategoryNames.NameOf(category).ToLowerInvariant() + ".md");

            File.Exists(overviewFile).Should().BeTrue(
                $"category overview page '{overviewFile}' must exist for rule {m.Id}");

            var text = File.ReadAllText(overviewFile);
            text.Should().Contain($"[{m.Id}]",
                $"{overviewFile} must link to rule {m.Id} (expected a '[{m.Id}](…)' markdown link)");
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "xaml-lint.slnx"))) return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not find repo root from test execution directory.");
    }
}
