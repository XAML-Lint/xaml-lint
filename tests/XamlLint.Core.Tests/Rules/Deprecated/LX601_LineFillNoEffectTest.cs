using XamlLint.Core.Rules.Deprecated;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Deprecated;

public sealed class LX601_LineFillNoEffectTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Line_with_Fill_literal_is_flagged()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Line [|Fill="Red"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Line_with_Fill_binding_is_flagged()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Line [|Fill="{Binding Tint}"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Line_without_Fill_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Line X1="0" Y1="0" X2="100" Y2="0" Stroke="Black" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Non_Line_element_with_Fill_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Rectangle Fill="Red" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Line_on_Wpf_dialect_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Line Fill="Red" />
            </Window>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX601 -->
                <Line Fill="Red" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_offending_lines_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX601_LineFillNoEffect>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Line [|Fill="Red"|] />
                <Line [|Fill="Blue"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
