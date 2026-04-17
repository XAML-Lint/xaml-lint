using XamlLint.Cli.Config;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Config;

public sealed class ConfigLoaderTest
{
    private static readonly IReadOnlyList<string> CatalogIds = new[]
    {
        "LX001", "LX002", "LX003", "LX004", "LX005", "LX006"
    };

    [Fact]
    public void Fallback_returns_recommended_severities_when_no_config_found()
    {
        using var tmp = new TempDir();
        var loader = new ConfigLoader();
        var result = loader.Discover(tmp.Path, CatalogIds);

        result.Config.Should().NotBeNull();
        result.Config!.RuleSeverities["LX001"].Should().Be(Severity.Error);
        result.Config.RuleSeverities["LX005"].Should().Be(Severity.Info);
    }

    [Fact]
    public void User_rule_severity_overrides_preset()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "xaml-lint.config.json"), """
            { "extends": "xaml-lint:recommended", "defaultDialect": "wpf", "rules": { "LX001": "warning" } }
            """);

        var loader = new ConfigLoader();
        var result = loader.Discover(tmp.Path, CatalogIds);

        result.Config!.RuleSeverities["LX001"].Should().Be(Severity.Warning);
    }

    [Fact]
    public void Unknown_rule_id_emits_LX003_warning_but_does_not_fail_load()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "xaml-lint.config.json"), """
            { "defaultDialect": "wpf", "rules": { "LX999": "error" } }
            """);

        var loader = new ConfigLoader();
        var result = loader.Discover(tmp.Path, CatalogIds);

        result.Config.Should().NotBeNull();
        result.Diagnostics.Should().Contain(d => d.RuleId == "LX003" && d.Severity == Severity.Warning);
    }

    [Fact]
    public void Malformed_json_returns_null_config_and_LX003_error()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "xaml-lint.config.json"), "{ not valid");

        var loader = new ConfigLoader();
        var result = loader.Discover(tmp.Path, CatalogIds);

        result.Config.Should().BeNull();
        result.Diagnostics.Should().Contain(d => d.RuleId == "LX003" && d.Severity == Severity.Error);
    }

    [Fact]
    public void Walk_up_stops_at_git_directory()
    {
        using var tmp = new TempDir();
        Directory.CreateDirectory(Path.Combine(tmp.Path, ".git"));
        var sub = Path.Combine(tmp.Path, "sub");
        Directory.CreateDirectory(sub);

        var loader = new ConfigLoader();
        var result = loader.Discover(sub, CatalogIds);
        result.Config.Should().NotBeNull();
    }

    [Fact]
    public void Global_star_applies_severity_to_every_rule()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "xaml-lint.config.json"), """
            { "defaultDialect": "wpf", "rules": { "*": "off" } }
            """);

        var loader = new ConfigLoader();
        var result = loader.Discover(tmp.Path, CatalogIds);

        result.Config!.RuleSeverities.Should().BeEmpty();
    }

    [Fact]
    public void Override_for_matching_glob_overlays_rule_severities()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "xaml-lint.config.json"), """
            {
              "defaultDialect": "wpf",
              "overrides": [
                { "files": "**/*.Designer.xaml", "rules": { "LX001": "off" } }
              ]
            }
            """);

        var loader = new ConfigLoader();
        var result = loader.Discover(tmp.Path, CatalogIds);

        var matched = ConfigLoader.ApplyOverridesForFile(result.Config!, Path.Combine(tmp.Path, "Foo.Designer.xaml"), tmp.Path);
        matched.ContainsKey("LX001").Should().BeFalse();

        var unmatched = ConfigLoader.ApplyOverridesForFile(result.Config!, Path.Combine(tmp.Path, "Foo.xaml"), tmp.Path);
        unmatched["LX001"].Should().Be(Severity.Error);
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("xaml-lint-test-").FullName;
        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }
}
