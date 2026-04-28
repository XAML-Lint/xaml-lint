using System.CommandLine;
using XamlLint.Cli;

namespace XamlLint.Cli.Tests;

public sealed class ProgramTest
{
    [Fact]
    public void Version_option_prints_ToolVersion_Current()
    {
        var root = Program.BuildRoot();
        root.Options.OfType<VersionOption>().Should().ContainSingle();

        using var sw = new StringWriter();
        var config = new InvocationConfiguration { Output = sw };
        var parseResult = root.Parse(new[] { "--version" });
        var exit = parseResult.Invoke(config);

        exit.Should().Be(0);
        sw.ToString().Trim().Should().Be(ToolVersion.Current);
    }
}
