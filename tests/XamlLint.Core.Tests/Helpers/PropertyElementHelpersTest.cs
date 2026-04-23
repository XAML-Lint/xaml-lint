using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class PropertyElementHelpersTest
{
    private const string WpfXmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    private static XElement Root(string xaml) =>
        XamlDocument.FromString(xaml, "f.xaml", Dialect.Wpf).Root!;

    [Fact]
    public void HasAttributeOrPropertyElement_matches_attribute_syntax()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" InputScope="Number" />
            """);

        PropertyElementHelpers.HasAttributeOrPropertyElement(textBox, "InputScope").Should().BeTrue();
    }

    [Fact]
    public void HasAttributeOrPropertyElement_matches_property_element_syntax()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}">
                <TextBox.InputScope>Number</TextBox.InputScope>
            </TextBox>
            """);

        PropertyElementHelpers.HasAttributeOrPropertyElement(textBox, "InputScope").Should().BeTrue();
    }

    [Fact]
    public void HasAttributeOrPropertyElement_matches_when_both_forms_present()
    {
        // Legal XAML forbids this (duplicate property), but helper should not crash and
        // should still return true. Behavior for subsequent value reads is covered by
        // GetAttributeOrPropertyElementValue.
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" InputScope="Number">
                <TextBox.InputScope>Url</TextBox.InputScope>
            </TextBox>
            """);

        PropertyElementHelpers.HasAttributeOrPropertyElement(textBox, "InputScope").Should().BeTrue();
    }

    [Fact]
    public void HasAttributeOrPropertyElement_returns_false_when_property_is_absent()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" />
            """);

        PropertyElementHelpers.HasAttributeOrPropertyElement(textBox, "InputScope").Should().BeFalse();
    }

    [Fact]
    public void HasAttributeOrPropertyElement_ignores_irrelevant_child_elements()
    {
        // A child element whose name does not end in ".InputScope" must not trigger a match,
        // even when other siblings exist.
        var grid = Root($"""
            <Grid xmlns="{WpfXmlns}">
                <TextBox />
                <Grid.RowDefinitions />
            </Grid>
            """);

        PropertyElementHelpers.HasAttributeOrPropertyElement(grid, "InputScope").Should().BeFalse();
    }

    [Fact]
    public void HasAttributeOrPropertyElement_matches_custom_element_type_suffix()
    {
        // Subclassed or renamed types write <MyTextBox.InputScope>; the suffix match handles
        // the general case without needing to know the element's own name.
        var textBox = Root($"""
            <MyTextBox xmlns="urn:test">
                <MyTextBox.InputScope>Number</MyTextBox.InputScope>
            </MyTextBox>
            """);

        PropertyElementHelpers.HasAttributeOrPropertyElement(textBox, "InputScope").Should().BeTrue();
    }

    [Fact]
    public void GetAttributeOrPropertyElementValue_returns_attribute_value_when_both_present()
    {
        // Attribute wins when both forms exist on the same element — consistent with how XAML
        // parsers surface the last-writer-wins resolution in practice.
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" InputScope="Number">
                <TextBox.InputScope>Url</TextBox.InputScope>
            </TextBox>
            """);

        PropertyElementHelpers.GetAttributeOrPropertyElementValue(textBox, "InputScope").Should().Be("Number");
    }

    [Fact]
    public void GetAttributeOrPropertyElementValue_returns_property_element_inner_text()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}">
                <TextBox.InputScope>Number</TextBox.InputScope>
            </TextBox>
            """);

        PropertyElementHelpers.GetAttributeOrPropertyElementValue(textBox, "InputScope").Should().Be("Number");
    }

    [Fact]
    public void GetAttributeOrPropertyElementValue_returns_null_when_property_is_absent()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" />
            """);

        PropertyElementHelpers.GetAttributeOrPropertyElementValue(textBox, "InputScope").Should().BeNull();
    }

    [Fact]
    public void GetAttributeOrPropertyElementValue_returns_attribute_value_when_only_attribute_present()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" InputScope="Number" />
            """);

        PropertyElementHelpers.GetAttributeOrPropertyElementValue(textBox, "InputScope").Should().Be("Number");
    }

    [Fact]
    public void TryGetValueAndSource_returns_attribute_source_for_attribute_form()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" InputScope="Number" />
            """);

        var result = PropertyElementHelpers.TryGetValueAndSource(textBox, "InputScope");
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be("Number");
        result.Value.Source.Should().BeOfType<XAttribute>();
    }

    [Fact]
    public void TryGetValueAndSource_returns_element_source_for_property_element_form()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}">
                <TextBox.InputScope>Number</TextBox.InputScope>
            </TextBox>
            """);

        var result = PropertyElementHelpers.TryGetValueAndSource(textBox, "InputScope");
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be("Number");
        result.Value.Source.Should().BeOfType<XElement>();
    }

    [Fact]
    public void TryGetValueAndSource_returns_null_when_property_is_absent()
    {
        var textBox = Root($"""
            <TextBox xmlns="{WpfXmlns}" />
            """);

        PropertyElementHelpers.TryGetValueAndSource(textBox, "InputScope").Should().BeNull();
    }
}
