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
    public void Image_with_SemanticProperties_Description_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="icon.png" SemanticProperties.Description="Cute dot net bot waving hi to you!" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_SemanticProperties_Hint_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="icon.png" SemanticProperties.Hint="Tap to return home." />
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
    public void Image_with_IsInAccessibleTree_True_is_still_flagged()
    {
        // IsInAccessibleTree="True" reasserts the default inclusion — the image IS in the
        // AT tree and still needs a name. Presence alone must not suppress the rule.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|Image|] Source="icon.png" AutomationProperties.IsInAccessibleTree="True" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_bound_IsInAccessibleTree_is_not_flagged()
    {
        // A bound value may resolve to False at runtime — don't second-guess it.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Image Source="icon.png" AutomationProperties.IsInAccessibleTree="{Binding IsDecorative}" />
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
    public void Image_on_Wpf_is_flagged()
    {
        // AutomationProperties.Name / HelpText / LabeledBy exist across WPF, WinUI 3, UWP,
        // Avalonia, Uno, and MAUI — the rule is a universal accessibility concern.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|Image|] Source="icon.png" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Image_on_Avalonia_is_flagged()
    {
        // Avalonia exposes AutomationProperties.Name / HelpText / LabeledBy via its
        // Automation infrastructure; the AutomationPeer relays them to the platform screen
        // reader.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <StackPanel xmlns="https://github.com/avaloniaui">
                <[|Image|] Source="icon.png" />
            </StackPanel>
            """,
            Dialect.Avalonia);
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

    [Fact]
    public void Image_with_dangling_LabeledBy_reference_is_flagged()
    {
        // After the XamlNameIndex retrofit, LabeledBy="{x:Reference Missing}" where no
        // element is named Missing must not suppress — the dangling reference is the bug.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <[|Image|] Source="icon.png" AutomationProperties.LabeledBy="{x:Reference MissingLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_cross_template_LabeledBy_reference_is_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <ContentPage.Resources>
                    <DataTemplate x:Key="t">
                        <Label x:Name="InnerLabel" Text="Home" />
                    </DataTemplate>
                </ContentPage.Resources>
                <[|Image|] Source="icon.png" AutomationProperties.LabeledBy="{x:Reference InnerLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_resolvable_ElementName_Binding_LabeledBy_is_not_flagged()
    {
        // {Binding ElementName=Foo} is statically resolvable just like {x:Reference Foo};
        // both forms scope-validate against XamlNameIndex.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Label x:Name="HomeLabel" Text="Home" />
                <Image Source="icon.png" AutomationProperties.LabeledBy="{Binding ElementName=HomeLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_dangling_ElementName_Binding_LabeledBy_is_flagged()
    {
        // Dangling ElementName must not suppress — the typo'd target is the bug.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <[|Image|] Source="icon.png" AutomationProperties.LabeledBy="{Binding ElementName=MissingLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_cross_template_ElementName_Binding_LabeledBy_is_flagged()
    {
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <ContentPage.Resources>
                    <DataTemplate x:Key="t">
                        <Label x:Name="InnerLabel" Text="Home" />
                    </DataTemplate>
                </ContentPage.Resources>
                <[|Image|] Source="icon.png" AutomationProperties.LabeledBy="{Binding ElementName=InnerLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_with_Binding_without_ElementName_still_suppresses()
    {
        // Pure data-binding expressions remain permissively suppressing — they can't be
        // statically evaluated to an element, but the author has stated intent.
        XamlDiagnosticVerifier<LX700_ImageWithoutAccessibleDescription>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Image Source="icon.png" AutomationProperties.LabeledBy="{Binding LabelElement}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
