using XamlLint.Cli.Formatters;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Formatters;

public sealed class PrettyFormatterTest
{
    [Fact]
    public void Clean_prints_no_issues_found()
    {
        var sw = new StringWriter();
        new PrettyFormatter(useColor: false).Write(sw, Array.Empty<Diagnostic>(), "0.1.0");
        sw.ToString().Trim().Should().Be("No issues found.");
    }

    [Fact]
    public void Header_per_file_groups_diagnostics()
    {
        var a = new Diagnostic("LX100", Severity.Warning, "m1", "a.xaml", 1, 1, 1, 1, null);
        var b = new Diagnostic("LX101", Severity.Warning, "m2", "a.xaml", 2, 1, 2, 1, null);
        var c = new Diagnostic("LX200", Severity.Info, "m3", "b.xaml", 1, 1, 1, 1, null);

        var sw = new StringWriter();
        new PrettyFormatter(useColor: false).Write(sw, new[] { a, b, c }, "0.1.0");

        var output = sw.ToString();
        output.Should().Contain("a.xaml");
        output.Should().Contain("b.xaml");
        output.Should().Contain("LX100");
        output.Should().Contain("LX200");
    }

    [Fact]
    public void Color_on_writes_escape_sequences()
    {
        var d = new Diagnostic("LX001", Severity.Error, "x", "a.xaml", 1, 1, 1, 1, null);
        var sw = new StringWriter();
        new PrettyFormatter(useColor: true).Write(sw, new[] { d }, "0.1.0");
        sw.ToString().Should().Contain("\u001b[");
    }

    [Fact]
    public void Color_off_suppresses_escape_sequences()
    {
        var d = new Diagnostic("LX001", Severity.Error, "x", "a.xaml", 1, 1, 1, 1, null);
        var sw = new StringWriter();
        new PrettyFormatter(useColor: false).Write(sw, new[] { d }, "0.1.0");
        sw.ToString().Should().NotContain("\u001b[");
    }
}
