using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX0505_PinWithoutLabelTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Pin_without_Label_on_Maui_is_flagged()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|Pin|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Pin_with_Label_literal_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Pin Label="Home" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Pin_with_Label_binding_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            """
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Pin Label="{Binding Name}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Pin_with_empty_Label_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Pin Label="" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Pin_with_Label_property_element_syntax_is_not_flagged()
    {
        // A <Pin.Label> property-element satisfies the runtime ArgumentException guardrail
        // just as clearly as the attribute form.
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Pin>
                    <Pin.Label>Home</Pin.Label>
                </Pin>
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Non_Pin_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <Label Text="hello" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Pin_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only. Dialect gating excludes WPF.
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Pin />
            </Window>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX0505 -->
                <Pin />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_offending_pins_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0505_PinWithoutLabel>.Analyze(
            $"""
            <ContentPage xmlns="{MauiXmlns}">
                <[|Pin|] />
                <[|Pin|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
