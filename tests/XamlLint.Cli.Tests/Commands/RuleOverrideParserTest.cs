using XamlLint.Cli.Commands;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Commands;

public sealed class RuleOverrideParserTest
{
    private static readonly IReadOnlyList<string> Catalog = new[] { "LX0001", "LX0100", "LX0200", "LX0700" };

    [Fact]
    public void Single_short_form_parses()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX0100:warning" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX0100"].Should().Be(Severity.Warning);
    }

    [Fact]
    public void Off_yields_null_sentinel()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX0100:off" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities.Should().ContainKey("LX0100");
        r.Severities["LX0100"].Should().BeNull();
    }

    [Fact]
    public void Csv_stacking_on_one_flag()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX0100:warning,LX0200:off" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX0100"].Should().Be(Severity.Warning);
        r.Severities["LX0200"].Should().BeNull();
    }

    [Fact]
    public void Repeated_flag_last_wins()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX0100:warning", "LX0100:error" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX0100"].Should().Be(Severity.Error);
    }

    [Fact]
    public void Unknown_rule_id_reports_error_with_id()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX0999:warning" }, Catalog);
        r.Errors.Should().ContainSingle().Which.Should().Contain("LX0999");
    }

    [Fact]
    public void Missing_severity_slot_is_error()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX0100" }, Catalog);
        r.Errors.Should().ContainSingle().Which.Should().Contain("ID:<severity>");
    }

    [Fact]
    public void Object_form_json_is_accepted()
    {
        var r = RuleOverrideParser.Parse(
            new[] { "{ \"LX0100\": \"warning\", \"LX0200\": \"off\" }" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX0100"].Should().Be(Severity.Warning);
        r.Severities["LX0200"].Should().BeNull();
    }

    [Fact]
    public void Object_form_rejects_unknown_rule_id()
    {
        var r = RuleOverrideParser.Parse(new[] { "{ \"LX0999\": \"warning\" }" }, Catalog);
        r.Errors.Should().ContainSingle().Which.Should().Contain("LX0999");
    }

    [Fact]
    public void Object_form_and_short_form_last_write_wins()
    {
        var r = RuleOverrideParser.Parse(
            new[] { "{ \"LX0100\": \"warning\" }", "LX0100:error" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX0100"].Should().Be(Severity.Error);
    }
}
