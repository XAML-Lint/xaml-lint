using System.Xml.Linq;
using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Tests.NameResolution;

public sealed class XamlNameReferenceScannerTest
{
    private const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";

    private static (XElement Root, XamlNameIndex Index) Build(string source)
    {
        var doc = XDocument.Parse(source, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
        var root = doc.Root!;
        return (root, XamlNameIndex.Build(root));
    }

    [Fact]
    public void Binding_ElementName_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox Text="{Binding ElementName=Header}" />
            </StackPanel>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(header);
    }

    [Fact]
    public void XReference_positional_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox DataContext="{x:Reference Header}" />
            </StackPanel>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(header);
    }

    [Fact]
    public void XReference_named_argument_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox DataContext="{x:Reference Name=Header}" />
            </StackPanel>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(header);
    }

    [Fact]
    public void Unprefixed_Reference_2009_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Label x:Name="HomeLabel" />
                <Entry Text="{Reference HomeLabel}" />
            </ContentPage>
            """);
        var homeLabel = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(homeLabel);
    }

    [Fact]
    public void Storyboard_TargetName_literal_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <Grid xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Rectangle x:Name="Box" />
                <Grid.Triggers>
                    <EventTrigger RoutedEvent="Grid.Loaded">
                        <BeginStoryboard>
                            <Storyboard>
                                <DoubleAnimation Storyboard.TargetName="Box"
                                                 Storyboard.TargetProperty="Opacity"
                                                 To="1" />
                            </Storyboard>
                        </BeginStoryboard>
                    </EventTrigger>
                </Grid.Triggers>
            </Grid>
            """);
        var box = root.Descendants().Single(e => e.Name.LocalName == "Rectangle");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(box);
    }

    [Fact]
    public void Setter_TargetName_literal_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <ControlTemplate xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}" TargetType="Button">
                <Border x:Name="Chrome">
                    <ContentPresenter />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="Chrome" Property="Background" Value="Red" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            """);
        var chrome = root.Descendants().Single(e => e.Name.LocalName == "Border");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(chrome);
    }

    [Fact]
    public void Trigger_SourceName_literal_marks_declaration_as_used()
    {
        var (root, index) = Build($$"""
            <ControlTemplate xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}" TargetType="Button">
                <Border x:Name="Chrome">
                    <CheckBox x:Name="Toggle" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger SourceName="Toggle" Property="IsChecked" Value="True">
                        <Setter TargetName="Chrome" Property="Background" Value="Green" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            """);
        var toggle = root.Descendants().Single(e => e.Name.LocalName == "CheckBox");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(toggle);
    }

    [Fact]
    public void Condition_SourceName_plain_literal_marks_declaration_as_used()
    {
        // MultiTrigger's nested <Condition> uses a plain SourceName="..." attribute in WPF.
        // The attached-property form ".SourceName" is exercised by a separate test.
        var (root, index) = Build($$"""
            <ControlTemplate xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}" TargetType="Button">
                <Border x:Name="Chrome">
                    <CheckBox x:Name="Toggle" />
                </Border>
                <ControlTemplate.Triggers>
                    <MultiTrigger>
                        <MultiTrigger.Conditions>
                            <Condition SourceName="Toggle" Property="IsChecked" Value="True" />
                        </MultiTrigger.Conditions>
                        <Setter TargetName="Chrome" Property="Background" Value="Green" />
                    </MultiTrigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            """);
        var toggle = root.Descendants().Single(e => e.Name.LocalName == "CheckBox");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(toggle);
    }

    [Fact]
    public void EventTrigger_SourceName_attached_property_literal_marks_declaration_as_used()
    {
        // Exercises the ".SourceName" suffix branch of IsNameReferenceAttribute — the
        // attached-property form used by WPF EventTrigger when it lives outside a
        // Style/ControlTemplate scope.
        var (root, index) = Build($$"""
            <Grid xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <CheckBox x:Name="Toggle" EventTrigger.SourceName="Toggle" />
            </Grid>
            """);
        var toggle = root.Descendants().Single(e => e.Name.LocalName == "CheckBox");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(toggle);
    }

    [Fact]
    public void Unreferenced_declaration_does_not_appear_in_used_set()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Orphan" />
                <TextBox />
            </StackPanel>
            """);
        var orphan = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(orphan);
    }

    [Fact]
    public void Cross_template_reference_does_not_resolve()
    {
        // Outer x:Name is not visible from inside a ControlTemplate; the reference inside
        // the template does not mark the outer declaration as used.
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="OuterLabel" />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <TextBlock Text="{Binding ElementName=OuterLabel}" />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
        var outerLabel = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(outerLabel);
    }

    [Fact]
    public void Reference_within_same_template_scope_resolves()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Border x:Name="Inner">
                                <TextBlock Text="{Binding ElementName=Inner, Path=Background}" />
                            </Border>
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
        var inner = root.Descendants().Single(e => e.Name.LocalName == "Border");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().Contain(inner);
    }

    [Fact]
    public void Reference_is_case_sensitive()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox Text="{Binding ElementName=header}" />
            </StackPanel>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(header);
    }

    [Fact]
    public void XBind_path_is_not_treated_as_a_reference()
    {
        // {x:Bind Header.Text} is a typed-path binding, not an element-name reference.
        // The rule intentionally does not parse these — LX0302 will report Header as unused
        // when only x:Bind touches it, and that is the documented limitation.
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox Text="{x:Bind Header.Text}" />
            </StackPanel>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(header);
    }

    [Fact]
    public void Empty_TargetName_value_does_not_resolve()
    {
        var (root, index) = Build($$"""
            <ControlTemplate xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}" TargetType="Button">
                <Border x:Name="Chrome" />
                <ControlTemplate.Triggers>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="" Property="Background" Value="Red" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            """);
        var chrome = root.Descendants().Single(e => e.Name.LocalName == "Border");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(chrome);
    }

    [Fact]
    public void Attribute_with_Name_suffix_but_not_TargetName_is_not_treated_as_a_reference()
    {
        // Guard against false positives on names like DataSourceName or UserName that end
        // in "Name" or "SourceName" via substring but are not XAML name references.
        var (root, index) = Build($$"""
            <Grid xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <UserControl DataSourceName="Header" UserName="Header" />
            </Grid>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(header);
    }

    [Fact]
    public void Self_declaration_is_not_counted_as_a_reference_to_itself()
    {
        // The Name/x:Name attribute on the declaring element is not a reference site.
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Lonely" />
            </StackPanel>
            """);
        var lonely = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().NotContain(lonely);
    }

    [Fact]
    public void Returns_empty_set_when_there_are_no_references()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="A" />
                <Label x:Name="B" />
                <Label x:Name="C" />
            </StackPanel>
            """);

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Should().BeEmpty();
    }

    [Fact]
    public void Multiple_references_to_same_declaration_yield_a_single_entry()
    {
        var (root, index) = Build($$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox Text="{Binding ElementName=Header}" />
                <CheckBox IsChecked="{Binding ElementName=Header, Path=IsEnabled}" />
            </StackPanel>
            """);
        var header = root.Descendants().Single(e => e.Name.LocalName == "Label");

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(root, index);

        used.Count.Should().Be(1);
        used.Should().Contain(header);
    }
}
