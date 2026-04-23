using XamlLint.Core.Rules.Accessibility;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Accessibility;

public sealed class LX701_ImageButtonWithoutAccessibleDescriptionTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void ImageButton_without_any_automation_attribute_is_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|ImageButton|] Source="icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <ImageButton Source="icon.png" AutomationProperties.Name="Close" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_AutomationProperties_HelpText_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <ImageButton Source="icon.png" AutomationProperties.HelpText="Close the current dialog." />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_SemanticProperties_Description_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <ImageButton Source="icon.png" SemanticProperties.Description="Close" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_SemanticProperties_Hint_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <ImageButton Source="icon.png" SemanticProperties.Hint="Close the current dialog." />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_AutomationProperties_LabeledBy_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Label x:Name="CloseLabel" Text="Close" />
                <ImageButton Source="icon.png" AutomationProperties.LabeledBy="{x:Reference CloseLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_excluded_from_accessibility_tree_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <ImageButton Source="icon.png" AutomationProperties.IsInAccessibleTree="False" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_IsInAccessibleTree_True_is_still_flagged()
    {
        // IsInAccessibleTree="True" reasserts the default inclusion — the button IS in the
        // AT tree and still needs a name. Presence alone must not suppress the rule.
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|ImageButton|] Source="icon.png" AutomationProperties.IsInAccessibleTree="True" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_bound_IsInAccessibleTree_is_not_flagged()
    {
        // A bound value may resolve to False at runtime — don't second-guess it.
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <ImageButton Source="icon.png" AutomationProperties.IsInAccessibleTree="{Binding IsDecorative}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_bound_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <ImageButton Source="icon.png" AutomationProperties.Name="{Binding ButtonLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_AutomationId_is_not_flagged()
    {
        // AutomationId is the canonical test-automation / UIA hook; any value signals the
        // author wired the button into automation, same rationale as LX700.
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <ImageButton Source="icon.png" AutomationId="DeleteBtn" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only. Dialect gating excludes WPF.
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <ImageButton Source="icon.png" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_ImageButton_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX701 -->
                <ImageButton Source="icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_image_buttons_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|ImageButton|] Source="icon.png" />
                <[|ImageButton|] Source="logo.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_dangling_LabeledBy_reference_is_flagged()
    {
        // After the XamlNameIndex retrofit, LabeledBy="{x:Reference Missing}" where no
        // element is named Missing must not suppress — the dangling reference is the bug.
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <[|ImageButton|] Source="icon.png" AutomationProperties.LabeledBy="{x:Reference MissingLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_cross_template_LabeledBy_reference_is_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <ContentPage.Resources>
                    <DataTemplate x:Key="t">
                        <Label x:Name="InnerLabel" Text="Home" />
                    </DataTemplate>
                </ContentPage.Resources>
                <[|ImageButton|] Source="icon.png" AutomationProperties.LabeledBy="{x:Reference InnerLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_resolvable_ElementName_Binding_LabeledBy_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Label x:Name="CloseLabel" Text="Close" />
                <ImageButton Source="icon.png" AutomationProperties.LabeledBy="{Binding ElementName=CloseLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_dangling_ElementName_Binding_LabeledBy_is_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <[|ImageButton|] Source="icon.png" AutomationProperties.LabeledBy="{Binding ElementName=MissingLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_cross_template_ElementName_Binding_LabeledBy_is_flagged()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <ContentPage.Resources>
                    <DataTemplate x:Key="t">
                        <Label x:Name="InnerLabel" Text="Close" />
                    </DataTemplate>
                </ContentPage.Resources>
                <[|ImageButton|] Source="icon.png" AutomationProperties.LabeledBy="{Binding ElementName=InnerLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ImageButton_with_Binding_without_ElementName_still_suppresses()
    {
        XamlDiagnosticVerifier<LX701_ImageButtonWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <ImageButton Source="icon.png" AutomationProperties.LabeledBy="{Binding LabelElement}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
