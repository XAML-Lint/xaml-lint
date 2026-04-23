using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class GridAncestryHelpersTest
{
    private const string WpfXmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    private static XamlDocument Doc(string xaml) =>
        XamlDocument.FromString(xaml, "f.xaml", Dialect.Wpf);

    [Fact]
    public void FindNearestGridAncestor_returns_null_when_element_has_no_grid_ancestor()
    {
        var doc = Doc($"""
            <StackPanel xmlns="{WpfXmlns}">
                <Button />
            </StackPanel>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        GridAncestryHelpers.FindNearestGridAncestor(button).Should().BeNull();
    }

    [Fact]
    public void FindNearestGridAncestor_returns_nearest_grid()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <StackPanel>
                    <Button />
                </StackPanel>
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        var grid = GridAncestryHelpers.FindNearestGridAncestor(button);
        grid.Should().NotBeNull();
        grid!.Name.LocalName.Should().Be("Grid");
    }

    [Fact]
    public void FindNearestGridAncestor_returns_innermost_grid_when_nested()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Grid>
                    <Button />
                </Grid>
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");
        var innerGrid = doc.Root!.Descendants().First(
            e => e.Name.LocalName == "Grid" && e.Parent?.Name.LocalName == "Grid");

        GridAncestryHelpers.FindNearestGridAncestor(button).Should().BeSameAs(innerGrid);
    }

    [Fact]
    public void CountRowDefinitions_returns_one_when_grid_has_no_definitions()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" />
            """);
        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(1);
    }

    [Fact]
    public void CountRowDefinitions_counts_row_definition_elements()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
            </Grid>
            """);
        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(3);
    }

    [Fact]
    public void CountRowDefinitions_uses_shorthand_attribute_when_present()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" RowDefinitions="Auto,*,Auto" />
            """);
        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(3);
    }

    [Fact]
    public void CountRowDefinitions_shorthand_attribute_ignores_empty_entries()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" RowDefinitions=",,Auto,,*,," />
            """);
        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(2);
    }

    [Fact]
    public void CountColumnDefinitions_counts_column_definition_elements()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
            </Grid>
            """);
        GridAncestryHelpers.CountColumnDefinitions(doc.Root!).Should().Be(2);
    }

    [Fact]
    public void CountColumnDefinitions_uses_shorthand_attribute_when_present()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" ColumnDefinitions="*,Auto" />
            """);
        GridAncestryHelpers.CountColumnDefinitions(doc.Root!).Should().Be(2);
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_returns_null_when_absent()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button />
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row").Should().BeNull();
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_reads_attribute_syntax_integer()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button Grid.Row="2" />
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        var read = GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row");
        read.Should().NotBeNull();
        read!.Value.Value.Should().Be(2);
        read.Value.Source.Should().BeOfType<XAttribute>();
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_reads_element_syntax_integer()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button>
                    <Grid.Row>2</Grid.Row>
                </Button>
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        var read = GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row");
        read.Should().NotBeNull();
        read!.Value.Value.Should().Be(2);
        read.Value.Source.Should().BeOfType<XElement>();
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_returns_null_when_value_is_not_an_integer()
    {
        // A markup-extension value like "{Binding Idx}" is intentionally not resolvable at lint
        // time; the helper returns null and the rule stays quiet.
        var doc = Doc($$"""
            <Grid xmlns="{{WpfXmlns}}"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button Grid.Row="{Binding Idx}" />
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row").Should().BeNull();
    }

    [Fact]
    public void CountColumnDefinitions_returns_one_when_grid_has_no_definitions()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" />
            """);
        GridAncestryHelpers.CountColumnDefinitions(doc.Root!).Should().Be(1);
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_reads_zero_value()
    {
        // Grid.Row="0" is the canonical "first row" value; the rule consumers check for
        // N >= rowCount, so zero must round-trip correctly.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button Grid.Row="0" />
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        var read = GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row");
        read.Should().NotBeNull();
        read!.Value.Value.Should().Be(0);
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_element_syntax_whitespace_only_value_is_ignored()
    {
        // The .Trim() defensive step inside the helper means a whitespace-only element-syntax
        // body parses to an empty string, which int.TryParse rejects. The helper returns null
        // rather than crashing.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button>
                    <Grid.Row>   </Grid.Row>
                </Button>
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row").Should().BeNull();
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_negative_integer_is_rejected()
    {
        // Negative indexes have no runtime meaning — the helper treats them the same as
        // unparseable values so the rule consumers do not see an `int` they cannot reason
        // about (LX100's `rowValue < rowCount` guard would otherwise silently accept -1).
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button Grid.Row="-1" />
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row").Should().BeNull();
    }

    [Fact]
    public void TryReadIntegerAttachedProperty_signed_integer_is_rejected()
    {
        // Explicit '+' prefixes are legal C# literals but unusual in XAML; rejecting them
        // keeps the accepted set tight (plain unsigned digits only).
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Button Grid.Row="+1" />
            </Grid>
            """);
        var button = doc.Root!.Descendants().First(e => e.Name.LocalName == "Button");

        GridAncestryHelpers.TryReadIntegerAttachedProperty(button, "Grid.Row").Should().BeNull();
    }

    [Fact]
    public void CountRowDefinitions_returns_one_when_row_definitions_element_is_empty()
    {
        // An empty <Grid.RowDefinitions /> is semantically identical to omitting the element
        // entirely — the Grid falls back to one implicit row.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}">
                <Grid.RowDefinitions />
            </Grid>
            """);
        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(1);
    }

    [Fact]
    public void CountRowDefinitions_ignores_shorthand_attribute_when_unsupported()
    {
        // When shorthand isn't supported (e.g. legacy-WPF), the RowDefinitions attribute is
        // treated as inert and we fall back to the element-syntax count (or implicit 1).
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" RowDefinitions="Auto,*,Auto" />
            """);

        GridAncestryHelpers.CountRowDefinitions(doc.Root!, shorthandSupported: false).Should().Be(1);
    }

    [Fact]
    public void CountRowDefinitions_uses_element_syntax_when_shorthand_unsupported_and_both_present()
    {
        // Defensive: if a legacy-WPF user wrote both forms, ignore shorthand and trust element-syntax.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" RowDefinitions="Auto,*,Auto">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
            </Grid>
            """);

        GridAncestryHelpers.CountRowDefinitions(doc.Root!, shorthandSupported: false).Should().Be(2);
    }

    [Fact]
    public void CountColumnDefinitions_ignores_shorthand_attribute_when_unsupported()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" ColumnDefinitions="*,Auto" />
            """);

        GridAncestryHelpers.CountColumnDefinitions(doc.Root!, shorthandSupported: false).Should().Be(1);
    }

    [Fact]
    public void CountRowDefinitions_element_syntax_wins_over_shorthand_when_both_declared()
    {
        // Matches upstream Rapid XAML Toolkit GridProcessor.cs:158 — the element-syntax form
        // is authoritative and the shorthand attribute is consulted only when element syntax
        // is absent or empty. A Grid declaring both is legal but unusual; this test pins the
        // precedence so a future refactor does not silently re-invert it.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" RowDefinitions="Auto,*">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
            </Grid>
            """);

        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(3);
    }

    [Fact]
    public void CountRowDefinitions_empty_element_syntax_falls_through_to_shorthand()
    {
        // Empty <Grid.RowDefinitions /> is parity with absence; the shorthand attribute takes
        // over. This matches upstream's "count > 0 → use element syntax; otherwise fall back"
        // pattern.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" RowDefinitions="Auto,*">
                <Grid.RowDefinitions />
            </Grid>
            """);

        GridAncestryHelpers.CountRowDefinitions(doc.Root!).Should().Be(2);
    }

    [Fact]
    public void CountColumnDefinitions_element_syntax_wins_over_shorthand_when_both_declared()
    {
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" ColumnDefinitions="Auto,*">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
            </Grid>
            """);

        GridAncestryHelpers.CountColumnDefinitions(doc.Root!).Should().Be(3);
    }

    [Fact]
    public void CountColumnDefinitions_default_overload_still_supports_shorthand()
    {
        // Existing call sites (no second arg) keep their behavior.
        var doc = Doc($"""
            <Grid xmlns="{WpfXmlns}" ColumnDefinitions="*,Auto" />
            """);

        GridAncestryHelpers.CountColumnDefinitions(doc.Root!).Should().Be(2);
    }
}
