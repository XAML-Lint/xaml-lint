using XamlLint.Core.Rules.Accessibility;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Accessibility;

public sealed class LX700_ImageWithoutAccessibleDescriptionTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Image_without_any_automation_attribute_is_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|Image|] Source="icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="icon.png" AutomationProperties.Name="Home" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_AutomationProperties_HelpText_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="icon.png" AutomationProperties.HelpText="Navigate to home page." />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_AutomationProperties_LabeledBy_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Label x:Name="HomeLabel" Text="Home" />
                <Image Source="icon.png" AutomationProperties.LabeledBy="{x:Reference HomeLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_excluded_from_accessibility_tree_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="divider.png" AutomationProperties.IsInAccessibleTree="False" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_bound_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Image Source="icon.png" AutomationProperties.Name="{Binding IconLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only. Dialect gating excludes WPF.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Image Source="icon.png" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_Image_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Label Text="hello" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX700 -->
                <Image Source="icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_images_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|Image|] Source="icon.png" />
                <[|Image|] Source="logo.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
