using System.CommandLine;
using XamlLint.Cli.Commands;

namespace XamlLint.Cli.Tests.Commands;

public sealed class UpdateCommandTest
{
    [Fact]
    public void Bare_invocation_parses_cleanly()
    {
        var cmd = UpdateCommand.Build();
        var result = cmd.Parse(Array.Empty<string>());

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Check_flag_parses_cleanly()
    {
        var cmd = UpdateCommand.Build();
        var result = cmd.Parse(new[] { "--check" });

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Unknown_argument_is_a_parse_error()
    {
        var cmd = UpdateCommand.Build();
        var result = cmd.Parse(new[] { "--bogus" });

        result.Errors.Should().NotBeEmpty();
    }
}
