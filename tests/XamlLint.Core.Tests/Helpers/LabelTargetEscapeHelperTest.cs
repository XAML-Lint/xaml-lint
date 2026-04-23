using System.Xml.Linq;
using XamlLint.Core.Helpers;
using XamlLint.Core.NameResolution;
using XamlLint.Core.Suppressions;

namespace XamlLint.Core.Tests.Helpers;

/// <summary>
/// Unit tests for <see cref="LabelTargetEscapeHelper"/>. These drive the reverse-labeling
/// logic directly, bypassing LX0702's name-escape so the semantics of the helper can be
/// verified without interference.
/// </summary>
public sealed class LabelTargetEscapeHelperTest
{
    [Fact]
    public void Label_with_x_Reference_Target_suppresses_referenced_element()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{x:Reference UsernameBox}">_User name:</Label>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeTrue();
    }

    [Fact]
    public void Label_with_Binding_ElementName_Target_suppresses_referenced_element()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{Binding ElementName=UsernameBox}">_User name:</Label>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeTrue();
    }

    [Fact]
    public void Label_targeting_a_different_name_does_not_suppress()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{x:Reference UsernameBox}">_User name:</Label>
                <TextBox x:Name="UsernameBox" />
                <TextBox x:Name="PasswordBox" />
            </StackPanel>
            """);
        var passwordBox = FindWithName(doc, "TextBox", "PasswordBox");

        LabelTargetEscapeHelper.Suppresses(passwordBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Unnamed_element_is_never_suppressed_by_Label_Target()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{x:Reference UsernameBox}">_User name:</Label>
                <TextBox />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Dangling_Label_Target_reference_does_not_suppress_anyone()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{x:Reference MissingBox}">_User name:</Label>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Label_inside_template_targeting_outer_name_does_not_suppress()
    {
        // Scope isolation: Label inside a ControlTemplate can only see names declared in
        // that template's scope, not the outer StackPanel scope.
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <StackPanel.Resources>
                    <ControlTemplate x:Key="t">
                        <Label Target="{x:Reference UsernameBox}">_User name:</Label>
                    </ControlTemplate>
                </StackPanel.Resources>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Binding_without_ElementName_does_not_resolve_to_any_element()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{Binding Path=UsernameBox}">_User name:</Label>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Literal_Target_value_does_not_resolve()
    {
        // A plain literal on Target wouldn't actually work at runtime for WPF's Label;
        // the helper is conservative and only treats markup-extension forms as references.
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="UsernameBox">_User name:</Label>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Label_with_no_Target_attribute_is_ignored()
    {
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label>User name:</Label>
                <TextBox x:Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeFalse();
    }

    [Fact]
    public void Unprefixed_Name_attribute_is_resolvable_via_Label_Target()
    {
        // XamlNameIndex indexes both x:Name and unprefixed Name; Label.Target should
        // resolve to either.
        var doc = Parse("""
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label Target="{Binding ElementName=UsernameBox}">_User name:</Label>
                <TextBox Name="UsernameBox" />
            </StackPanel>
            """);
        var textBox = Find(doc, "TextBox");

        LabelTargetEscapeHelper.Suppresses(textBox, ContextFor(doc)).Should().BeTrue();
    }

    private static XDocument Parse(string xml) => XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

    private static XElement Find(XDocument doc, string localName) =>
        doc.Root!.DescendantsAndSelf().First(e => e.Name.LocalName == localName);

    private static XElement FindWithName(XDocument doc, string localName, string nameValue) =>
        doc.Root!.DescendantsAndSelf()
            .First(e => e.Name.LocalName == localName
                && (e.Attributes().Any(a => a.Name.LocalName == "Name" && a.Value == nameValue)));

    private static RuleContext ContextFor(XDocument doc) => new()
    {
        Dialect = Dialect.Wpf,
        SeverityMap = new Dictionary<string, Severity>(),
        Suppressions = new SuppressionMap(),
        Source = ReadOnlyMemory<char>.Empty,
        NameIndexBuilder = () => XamlNameIndex.Build(doc.Root!),
    };
}
