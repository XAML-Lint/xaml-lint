using System.Xml.Linq;
using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Tests.NameResolution;

public sealed class XamlNameIndexTest
{
    private const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";

    private static XDocument Parse(string source) =>
        XDocument.Parse(source, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);

    [Fact]
    public void Resolves_root_scoped_x_name()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="Header" />
                <TextBox x:Name="Input" />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "Header").Should().BeTrue();
        index.IsDefinedInScopeOf(textBox, "Input").Should().BeTrue();
        index.IsDefinedInScopeOf(textBox, "Missing").Should().BeFalse();
    }

    [Fact]
    public void Resolves_unprefixed_Name_attribute()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}">
                <Label Name="Header" />
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "Header").Should().BeTrue();
    }

    [Fact]
    public void Empty_or_whitespace_name_is_not_registered()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="" />
                <Label x:Name="   " />
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "").Should().BeFalse();
        index.IsDefinedInScopeOf(textBox, "   ").Should().BeFalse();
    }

    [Fact]
    public void Names_are_case_sensitive()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="Header" />
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "Header").Should().BeTrue();
        index.IsDefinedInScopeOf(textBox, "header").Should().BeFalse();
        index.IsDefinedInScopeOf(textBox, "HEADER").Should().BeFalse();
    }

    [Fact]
    public void Name_inside_ControlTemplate_is_isolated_from_outer_scope()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Border x:Name="InnerBorder" />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");
        var innerBorder = doc.Root!.Descendants().Single(e => e.Name.LocalName == "Border");

        index.IsDefinedInScopeOf(textBox, "InnerBorder").Should().BeFalse();
        index.IsDefinedInScopeOf(innerBorder, "InnerBorder").Should().BeTrue();
    }

    [Fact]
    public void Outer_name_is_not_visible_from_inside_a_template()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="OuterLabel" />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Border x:Name="InnerBorder" />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var innerBorder = doc.Root!.Descendants().Single(e => e.Name.LocalName == "Border");

        index.IsDefinedInScopeOf(innerBorder, "OuterLabel").Should().BeFalse();
    }

    [Fact]
    public void DataTemplate_opens_a_nested_scope()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <ListBox>
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <TextBlock x:Name="ItemText" />
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "ItemText").Should().BeFalse();
    }

    [Fact]
    public void ItemsPanelTemplate_opens_a_nested_scope()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <ListBox>
                    <ListBox.ItemsPanel>
                        <ItemsPanelTemplate>
                            <StackPanel x:Name="Panel" />
                        </ItemsPanelTemplate>
                    </ListBox.ItemsPanel>
                </ListBox>
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "Panel").Should().BeFalse();
    }

    [Fact]
    public void HierarchicalDataTemplate_opens_a_nested_scope()
    {
        var doc = Parse($"""
            <TreeView xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <TreeView.ItemTemplate>
                    <HierarchicalDataTemplate>
                        <TextBlock x:Name="NodeText" />
                    </HierarchicalDataTemplate>
                </TreeView.ItemTemplate>
            </TreeView>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var root = doc.Root!;

        index.IsDefinedInScopeOf(root, "NodeText").Should().BeFalse();
    }

    [Fact]
    public void Nested_templates_isolate_from_parent_template_and_root()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="Root" />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Grid x:Name="OuterTpl">
                                <ContentPresenter>
                                    <ContentPresenter.ContentTemplate>
                                        <DataTemplate>
                                            <TextBlock x:Name="Inner" />
                                        </DataTemplate>
                                    </ContentPresenter.ContentTemplate>
                                </ContentPresenter>
                            </Grid>
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var inner = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBlock");

        index.IsDefinedInScopeOf(inner, "Inner").Should().BeTrue();
        index.IsDefinedInScopeOf(inner, "OuterTpl").Should().BeFalse();
        index.IsDefinedInScopeOf(inner, "Root").Should().BeFalse();
    }

    [Fact]
    public void Duplicate_name_in_same_scope_does_not_throw()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="Dup" />
                <Label x:Name="Dup" />
                <TextBox />
            </StackPanel>
            """);
        var act = () => XamlNameIndex.Build(doc.Root!);
        act.Should().NotThrow();

        var index = act();
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");
        index.IsDefinedInScopeOf(textBox, "Dup").Should().BeTrue();
    }

    [Fact]
    public void AllNames_enumerates_every_scope()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="Outer" />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Border x:Name="Inner" />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);

        var names = index.AllNames().Select(x => x.Name).OrderBy(n => n).ToList();
        names.Should().Equal("Inner", "Outer");
    }

    [Fact]
    public void Document_with_no_names_is_empty()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}">
                <Label />
                <TextBox />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var textBox = doc.Root!.Descendants().Single(e => e.Name.LocalName == "TextBox");

        index.IsDefinedInScopeOf(textBox, "Anything").Should().BeFalse();
        index.AllNames().Should().BeEmpty();
    }

    [Fact]
    public void Handles_maui_xaml2009_namespace_for_x_name()
    {
        const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
        var doc = Parse($"""
            <ContentPage xmlns="{Maui}" xmlns:x="{Xaml2009}">
                <Label x:Name="HomeLabel" Text="Home" />
                <Entry />
            </ContentPage>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var entry = doc.Root!.Descendants().Single(e => e.Name.LocalName == "Entry");

        index.IsDefinedInScopeOf(entry, "HomeLabel").Should().BeTrue();
    }

    [Fact]
    public void Reference_element_outside_the_document_tree_returns_false()
    {
        var doc = Parse($"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <Label x:Name="Header" />
            </StackPanel>
            """);
        var index = XamlNameIndex.Build(doc.Root!);
        var orphan = new XElement("Orphan");

        index.IsDefinedInScopeOf(orphan, "Header").Should().BeFalse();
    }
}
