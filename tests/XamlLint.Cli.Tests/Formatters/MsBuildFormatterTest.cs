using XamlLint.Cli.Formatters;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Formatters;

public sealed class MsBuildFormatterTest
{
    [Fact]
    public void Clean_results_produce_empty_output()
    {
        var sw = new StringWriter();
        new MsBuildFormatter().Write(sw, Array.Empty<Diagnostic>(), "0.1.0");
        sw.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Diagnostic_is_formatted_per_spec()
    {
        var d = new Diagnostic("LX300", Severity.Warning, "x:Name 'myButton' should start with uppercase.", "src/Views/MainView.xaml", 12, 28, 12, 38, "https://help");
        var sw = new StringWriter();
        new MsBuildFormatter().Write(sw, new[] { d }, "0.1.0");

        var line = sw.ToString().TrimEnd();
        line.Should().Be("src/Views/MainView.xaml(12,28): warning LX300: x:Name 'myButton' should start with uppercase. [https://help]");
    }

    [Fact]
    public void HelpUri_is_omitted_when_null()
    {
        var d = new Diagnostic("LX001", Severity.Error, "bad", "f.xaml", 1, 1, 1, 1, null);
        var sw = new StringWriter();
        new MsBuildFormatter().Write(sw, new[] { d }, "0.1.0");

        sw.ToString().TrimEnd().Should().Be("f.xaml(1,1): error LX001: bad");
    }
}
