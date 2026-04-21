using XamlLint.Core.Rules.Accessibility;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Accessibility;

public sealed class LX702_TextBoxWithoutAccessibleDescriptionTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void Bare_TextBox_on_Wpf_is_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <[|TextBox|] />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_x_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}" xmlns:x="{Xaml2006}">
                <TextBox x:Name="Input" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_unprefixed_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <TextBox Name="Input" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_Header_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <TextBox Header="Username" />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_with_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <TextBox AutomationProperties.Name="Username" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_resolvable_x_Reference_LabeledBy_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label x:Name="UsernameLabel" Content="Username" />
                <TextBox AutomationProperties.LabeledBy="{x:Reference UsernameLabel}" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_dangling_x_Reference_LabeledBy_is_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <[|TextBox|] AutomationProperties.LabeledBy="{x:Reference UsernameLabel}" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_x_Reference_across_template_boundary_is_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <ListBox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <Label x:Name="ItemLabel" />
                    </DataTemplate>
                </ListBox.ItemTemplate>
                <ListBox.Resources>
                    <DataTemplate x:Key="t">
                        <[|TextBox|] AutomationProperties.LabeledBy="{x:Reference ItemLabel}" />
                    </DataTemplate>
                </ListBox.Resources>
            </ListBox>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_Binding_in_LabeledBy_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox AutomationProperties.LabeledBy="{Binding LabelElement}" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_non_reference_literal_LabeledBy_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <TextBox AutomationProperties.LabeledBy="UsernameLabel" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_on_Maui_dialect_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <TextBox />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <!-- xaml-lint disable once LX702 -->
                <TextBox />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_resolvable_ElementName_Binding_LabeledBy_is_not_flagged()
    {
        // {Binding ElementName=Foo} is the dominant WPF element-reference idiom
        // (x:Reference arrived later with XAML 2009). Treat it as equivalent for
        // LabeledBy scope validation.
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Label x:Name="UsernameLabel" Content="Username" />
                <TextBox AutomationProperties.LabeledBy="{Binding ElementName=UsernameLabel}" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_dangling_ElementName_Binding_LabeledBy_is_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <[|TextBox|] AutomationProperties.LabeledBy="{Binding ElementName=MissingLabel}" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_cross_template_ElementName_Binding_LabeledBy_is_flagged()
    {
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <ListBox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <Label x:Name="ItemLabel" />
                    </DataTemplate>
                </ListBox.ItemTemplate>
                <ListBox.Resources>
                    <DataTemplate x:Key="t">
                        <[|TextBox|] AutomationProperties.LabeledBy="{Binding ElementName=ItemLabel}" />
                    </DataTemplate>
                </ListBox.Resources>
            </ListBox>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_with_Binding_without_ElementName_still_suppresses()
    {
        // Pure data-binding (no ElementName) can't be statically resolved to an element;
        // the permissive "suppress" behaviour is retained.
        XamlDiagnosticVerifier<LX702_TextBoxWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox AutomationProperties.LabeledBy="{Binding LabelElement}" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }
}
