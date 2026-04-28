using System.CommandLine;
using System.CommandLine.Invocation;
using XamlLint.Cli;

namespace XamlLint.Cli.Tests;

public sealed class ProgramTest
{
    [Fact]
    public void Version_option_prints_ToolVersion_Current()
    {
        var root = Program.BuildRoot();
        var versionOption = root.Options.OfType<VersionOption>().Single();

        var originalOut = Console.Out;
        using var sw = new StringWriter();
        try
        {
            Console.SetOut(sw);
            var parseResult = root.Parse(new[] { "--version" });
            var exit = parseResult.Invoke();
            exit.Should().Be(0);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        sw.ToString().Trim().Should().Be(ToolVersion.Current);
    }
}
