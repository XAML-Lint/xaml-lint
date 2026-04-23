using System.Text.Json;
using XamlLint.Cli.Formatters;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Formatters;

public sealed class CompactJsonFormatterTest
{
    [Fact]
    public void Empty_diagnostics_still_emits_envelope()
    {
        var sw = new StringWriter();
        new CompactJsonFormatter().Write(sw, Array.Empty<Diagnostic>(), "0.1.0");

        var json = JsonDocument.Parse(sw.ToString()).RootElement;
        json.GetProperty("version").GetString().Should().Be("1");
        json.GetProperty("results").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void Diagnostic_fields_map_one_to_one()
    {
        var diag = new Diagnostic("LX0300", Severity.Warning, "msg", "f.xaml", 12, 28, 12, 38, "https://help");
        var sw = new StringWriter();
        new CompactJsonFormatter().Write(sw, new[] { diag }, "0.1.0");

        var root = JsonDocument.Parse(sw.ToString()).RootElement;
        var first = root.GetProperty("results")[0];
        first.GetProperty("file").GetString().Should().Be("f.xaml");
        first.GetProperty("ruleId").GetString().Should().Be("LX0300");
        first.GetProperty("severity").GetString().Should().Be("warning");
        first.GetProperty("startLine").GetInt32().Should().Be(12);
        first.GetProperty("helpUri").GetString().Should().Be("https://help");
    }

    [Fact]
    public void Tool_version_surfaces_in_envelope()
    {
        var sw = new StringWriter();
        new CompactJsonFormatter().Write(sw, Array.Empty<Diagnostic>(), "1.2.3");

        var root = JsonDocument.Parse(sw.ToString()).RootElement;
        root.GetProperty("tool").GetProperty("version").GetString().Should().Be("1.2.3");
        root.GetProperty("tool").GetProperty("name").GetString().Should().Be("xaml-lint");
    }
}
