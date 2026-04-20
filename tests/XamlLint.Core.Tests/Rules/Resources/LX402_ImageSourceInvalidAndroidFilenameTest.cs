using XamlLint.Core.Rules.Resources;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Resources;

public sealed class LX402_ImageSourceInvalidAndroidFilenameTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Uppercase_filename_is_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="MyIcon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Hyphen_in_filename_is_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="my-icon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Space_in_filename_is_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="my icon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Leading_digit_is_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="2x_icon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Lowercase_filename_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Underscore_only_filename_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="my_icon_2.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Nested_folder_only_checks_leaf_name()
    {
        // Folder segment is uppercase; leaf is lowercase — only leaf is checked.
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="Resources/icon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Nested_folder_with_invalid_leaf_is_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="Resources/MyIcon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Backslash_separator_only_checks_leaf_name()
    {
        // Backslash is also treated as a path separator; leaf is invalid.
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="Resources\MyIcon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Binding_source_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Image Source="{Binding IconPath}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void StaticResource_source_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Image Source="{StaticResource DefaultIcon}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Http_uri_source_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="https://example.com/MyIcon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void MsAppx_uri_source_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="ms-appx:///Assets/MyIcon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void File_uri_source_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image Source="file:///Users/me/MyIcon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Missing_Source_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Image_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only. Dialect gating excludes WPF.
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Image Source="MyIcon.png" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX402 -->
                <Image Source="MyIcon.png" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_offending_images_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX402_ImageSourceInvalidAndroidFilename>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Image [|Source="MyIcon.png"|] />
                <Image [|Source="my-other-icon.png"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
