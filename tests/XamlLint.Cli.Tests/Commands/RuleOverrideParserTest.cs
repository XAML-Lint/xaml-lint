using XamlLint.Cli.Commands;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Commands;

public sealed class RuleOverrideParserTest
{
    private static readonly IReadOnlyList<string> Catalog = new[] { "LX001", "LX100", "LX200", "LX700" };

    [Fact]
    public void Single_short_form_parses()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX100:warning" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX100"].Should().Be(Severity.Warning);
    }

    [Fact]
    public void Off_yields_null_sentinel()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX100:off" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities.Should().ContainKey("LX100");
        r.Severities["LX100"].Should().BeNull();
    }

    [Fact]
    public void Csv_stacking_on_one_flag()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX100:warning,LX200:off" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX100"].Should().Be(Severity.Warning);
        r.Severities["LX200"].Should().BeNull();
    }

    [Fact]
    public void Repeated_flag_last_wins()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX100:warning", "LX100:error" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX100"].Should().Be(Severity.Error);
    }

    [Fact]
    public void Unknown_rule_id_reports_error_with_id()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX999:warning" }, Catalog);
        r.Errors.Should().ContainSingle().Which.Should().Contain("LX999");
    }

    [Fact]
    public void Missing_severity_slot_is_error()
    {
        var r = RuleOverrideParser.Parse(new[] { "LX100" }, Catalog);
        r.Errors.Should().ContainSingle().Which.Should().Contain("ID:<severity>");
    }

    [Fact]
    public void Object_form_json_is_accepted()
    {
        var r = RuleOverrideParser.Parse(
            new[] { "{ \"LX100\": \"warning\", \"LX200\": \"off\" }" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX100"].Should().Be(Severity.Warning);
        r.Severities["LX200"].Should().BeNull();
    }

    [Fact]
    public void Object_form_rejects_unknown_rule_id()
    {
        var r = RuleOverrideParser.Parse(new[] { "{ \"LX999\": \"warning\" }" }, Catalog);
        r.Errors.Should().ContainSingle().Which.Should().Contain("LX999");
    }

    [Fact]
    public void Object_form_and_short_form_last_write_wins()
    {
        var r = RuleOverrideParser.Parse(
            new[] { "{ \"LX100\": \"warning\" }", "LX100:error" }, Catalog);
        r.Errors.Should().BeEmpty();
        r.Severities["LX100"].Should().Be(Severity.Error);
    }
}
