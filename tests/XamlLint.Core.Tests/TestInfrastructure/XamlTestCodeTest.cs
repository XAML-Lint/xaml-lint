namespace XamlLint.Core.Tests.TestInfrastructure;

public sealed class XamlTestCodeTest
{
    [Fact]
    public void Clean_source_has_no_markers()
    {
        var parsed = XamlTestCode.Parse("<Grid/>");
        parsed.Source.Should().Be("<Grid/>");
        parsed.Expected.Should().BeEmpty();
    }

    [Fact]
    public void Full_marker_records_rule_and_span()
    {
        var parsed = XamlTestCode.Parse("<Grid {|LX0100:Row=\"1\"|} />");
        parsed.Source.Should().Be("<Grid Row=\"1\" />");
        parsed.Expected.Should().ContainSingle().Which.RuleId.Should().Be("LX0100");
    }

    [Fact]
    public void Spanless_marker_records_rule_without_position()
    {
        var parsed = XamlTestCode.Parse("<Grid{|LX0100|}/>");
        parsed.Source.Should().Be("<Grid/>");
        parsed.Expected.Should().ContainSingle().Which.HasSpan.Should().BeFalse();
    }

    [Fact]
    public void Shorthand_marker_assumes_the_default_rule_id()
    {
        var parsed = XamlTestCode.Parse("<Grid [|Row=\"1\"|] />", defaultRuleId: "LX0100");
        parsed.Source.Should().Be("<Grid Row=\"1\" />");
        parsed.Expected.Should().ContainSingle().Which.RuleId.Should().Be("LX0100");
    }

    [Fact]
    public void Shorthand_without_default_throws()
    {
        var act = () => XamlTestCode.Parse("<Grid [|Row=\"1\"|] />");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiple_markers_record_multiple_expectations()
    {
        var parsed = XamlTestCode.Parse("<Grid {|LX0100:Row=\"1\"|} {|LX0101:Col=\"2\"|} />");
        parsed.Expected.Should().HaveCount(2);
        parsed.Source.Should().Be("<Grid Row=\"1\" Col=\"2\" />");
    }
}
