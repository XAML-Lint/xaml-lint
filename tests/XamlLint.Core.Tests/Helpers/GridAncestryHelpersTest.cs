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
}
