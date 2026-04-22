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

    [Fact]
    public void Rule_short_form_parses_cleanly()
    {
        var cmd = LintCommand.Build();
        var r = cmd.Parse(new[] { "--rule", "LX100:warning", "foo.xaml" });
        r.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Preset_unknown_is_a_parse_error()
    {
        var cmd = LintCommand.Build();
        var r = cmd.Parse(new[] { "--preset", "bogus", "foo.xaml" });
        r.Errors.Should().NotBeEmpty();
        r.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("--preset") && m.Contains("bogus"));
    }

    [Fact]
    public void Preset_known_value_parses_cleanly()
    {
        var cmd = LintCommand.Build();
        cmd.Parse(new[] { "--preset", "recommended", "foo.xaml" }).Errors.Should().BeEmpty();
        cmd.Parse(new[] { "--preset", "strict", "foo.xaml" }).Errors.Should().BeEmpty();
        cmd.Parse(new[] { "--preset", "none", "foo.xaml" }).Errors.Should().BeEmpty();
    }

    [Fact]
    public void Only_is_mutually_exclusive_with_preset()
    {
        var cmd = LintCommand.Build();
        var r = cmd.Parse(new[] { "--only", "LX100", "--preset", "strict", "foo.xaml" });
        r.Errors.Should().NotBeEmpty();
        r.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("--only") && m.Contains("--preset"));
    }

    [Fact]
    public void Only_is_mutually_exclusive_with_rule()
    {
        var cmd = LintCommand.Build();
        var r = cmd.Parse(new[] { "--only", "LX100", "--rule", "LX200:warning", "foo.xaml" });
        r.Errors.Should().NotBeEmpty();
        r.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("--only") && m.Contains("--rule"));
    }

    [Fact]
    public void Config_short_alias_dash_c_is_accepted()
    {
        var cmd = LintCommand.Build();
        var r = cmd.Parse(new[] { "-c", "custom.json", "foo.xaml" });
        r.Errors.Should().BeEmpty();
    }

    [Fact]
    public void No_inline_config_parses_cleanly()
    {
        var cmd = LintCommand.Build();
        var r = cmd.Parse(new[] { "--no-inline-config", "foo.xaml" });
        r.Errors.Should().BeEmpty();
    }

    [Fact]
    public void No_config_lookup_parses_cleanly()
    {
        var cmd = LintCommand.Build();
        cmd.Parse(new[] { "--no-config-lookup", "foo.xaml" }).Errors.Should().BeEmpty();
    }
}
