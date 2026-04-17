using System.Text.Json;
using XamlLint.Cli.Formatters;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Formatters;

public sealed class SarifFormatterTest
{
    [Fact]
    public void Sarif_2_1_0_envelope_has_required_fields()
    {
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, Array.Empty<Diagnostic>(), "0.1.0");

        var root = JsonDocument.Parse(sw.ToString()).RootElement;
        root.GetProperty("version").GetString().Should().Be("2.1.0");
        root.GetProperty("$schema").GetString().Should().Contain("sarif");
        root.GetProperty("runs").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public void Tool_driver_has_name_version_informationUri()
    {
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, Array.Empty<Diagnostic>(), "0.1.0");

        var driver = JsonDocument.Parse(sw.ToString()).RootElement
            .GetProperty("runs")[0]
            .GetProperty("tool")
            .GetProperty("driver");

        driver.GetProperty("name").GetString().Should().Be("xaml-lint");
        driver.GetProperty("version").GetString().Should().Be("0.1.0");
        driver.GetProperty("informationUri").GetString().Should().Contain("xaml-lint");
    }

    [Fact]
    public void Severity_maps_to_sarif_level()
    {
        var diags = new[]
        {
            new Diagnostic("LX001", Severity.Error, "e", "f.xaml", 1, 1, 1, 1, null),
            new Diagnostic("LX002", Severity.Warning, "w", "f.xaml", 1, 1, 1, 1, null),
            new Diagnostic("LX003", Severity.Info, "i", "f.xaml", 1, 1, 1, 1, null),
        };
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, diags, "0.1.0");

        var results = JsonDocument.Parse(sw.ToString()).RootElement
            .GetProperty("runs")[0]
            .GetProperty("results");

        results[0].GetProperty("level").GetString().Should().Be("error");
        results[1].GetProperty("level").GetString().Should().Be("warning");
        results[2].GetProperty("level").GetString().Should().Be("note");
    }

    [Fact]
    public void Result_locations_use_one_based_lines_and_columns()
    {
        var d = new Diagnostic("LX100", Severity.Warning, "m", "Views/A.xaml", 5, 7, 5, 12, null);
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, new[] { d }, "0.1.0");

        var result = JsonDocument.Parse(sw.ToString()).RootElement
            .GetProperty("runs")[0]
            .GetProperty("results")[0];

        var region = result.GetProperty("locations")[0].GetProperty("physicalLocation").GetProperty("region");
        region.GetProperty("startLine").GetInt32().Should().Be(5);
        region.GetProperty("startColumn").GetInt32().Should().Be(7);
        region.GetProperty("endLine").GetInt32().Should().Be(5);
        region.GetProperty("endColumn").GetInt32().Should().Be(12);

        var uri = result.GetProperty("locations")[0].GetProperty("physicalLocation").GetProperty("artifactLocation").GetProperty("uri").GetString();
        uri.Should().Be("Views/A.xaml");
    }

    [Fact]
    public void Rules_entry_has_title_and_helpUri_from_catalog()
    {
        var d = new Diagnostic("LX001", Severity.Error, "m", "f.xaml", 1, 1, 1, 1, HelpUri: null);
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, new[] { d }, "0.1.0");

        var rules = JsonDocument.Parse(sw.ToString()).RootElement
            .GetProperty("runs")[0]
            .GetProperty("tool")
            .GetProperty("driver")
            .GetProperty("rules");

        rules.GetArrayLength().Should().Be(1);
        rules[0].GetProperty("id").GetString().Should().Be("LX001");
        rules[0].GetProperty("name").GetString().Should().Be("Malformed XAML");
        rules[0].GetProperty("shortDescription").GetProperty("text").GetString().Should().Be("Malformed XAML");
        rules[0].GetProperty("helpUri").GetString().Should().Contain("LX001");
    }

    [Fact]
    public void Rules_entry_falls_back_to_id_for_unknown_rule()
    {
        var d = new Diagnostic("LX999", Severity.Warning, "m", "f.xaml", 1, 1, 1, 1, null);
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, new[] { d }, "0.1.0");

        var rule = JsonDocument.Parse(sw.ToString()).RootElement
            .GetProperty("runs")[0]
            .GetProperty("tool")
            .GetProperty("driver")
            .GetProperty("rules")[0];

        rule.GetProperty("name").GetString().Should().Be("LX999");
        rule.TryGetProperty("helpUri", out _).Should().BeFalse();
    }

    [Fact]
    public void Suppressions_array_emits_when_provided()
    {
        var d = new Diagnostic("LX100", Severity.Warning, "m", "A.xaml", 1, 1, 1, 1, null);
        var sw = new StringWriter();
        new SarifFormatter().Write(sw, Array.Empty<Diagnostic>(), "0.1.0", suppressed: new[] { d });

        var result = JsonDocument.Parse(sw.ToString()).RootElement
            .GetProperty("runs")[0]
            .GetProperty("results")[0];

        var suppressions = result.GetProperty("suppressions");
        suppressions.GetArrayLength().Should().Be(1);
        suppressions[0].GetProperty("kind").GetString().Should().Be("inSource");
    }
}
