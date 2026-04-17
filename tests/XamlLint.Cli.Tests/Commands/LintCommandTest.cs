using System.CommandLine;
using XamlLint.Cli.Commands;

namespace XamlLint.Cli.Tests.Commands;

public sealed class LintCommandTest
{
    [Fact]
    public void Unknown_format_is_surfaced_as_parse_error()
    {
        var cmd = LintCommand.Build();
        var result = cmd.Parse(new[] { "--format", "bogus", "foo.xaml" });

        result.Errors.Should().NotBeEmpty();
        result.Errors.Select(e => e.Message)
            .Should().Contain(m => m.Contains("--format") && m.Contains("bogus"));
    }

    [Fact]
    public void Unknown_verbosity_is_surfaced_as_parse_error()
    {
        var cmd = LintCommand.Build();
        var result = cmd.Parse(new[] { "--verbosity", "bogus", "foo.xaml" });

        result.Errors.Should().NotBeEmpty();
        result.Errors.Select(e => e.Message)
            .Should().Contain(m => m.Contains("--verbosity") && m.Contains("bogus"));
    }

    [Fact]
    public void Known_format_parses_cleanly()
    {
        var cmd = LintCommand.Build();
        var result = cmd.Parse(new[] { "--format", "sarif", "foo.xaml" });

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Known_verbosity_alias_parses_cleanly()
    {
        var cmd = LintCommand.Build();
        var result = cmd.Parse(new[] { "--verbosity", "diag", "foo.xaml" });

        result.Errors.Should().BeEmpty();
    }
}
