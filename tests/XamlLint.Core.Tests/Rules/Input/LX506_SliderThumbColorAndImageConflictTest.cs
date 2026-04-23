using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX506_SliderThumbColorAndImageConflictTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Slider_with_both_thumb_properties_is_flagged_on_ThumbColor()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider [|ThumbColor="Red"|] ThumbImageSource="thumb.png" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_both_bound_is_flagged()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            """
            <StackLayout xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Slider [|ThumbColor="{Binding Tint}"|] ThumbImageSource="{Binding ThumbImage}" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_mixed_literal_and_bound_is_flagged()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            """
            <StackLayout xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Slider [|ThumbColor="Red"|] ThumbImageSource="{Binding ThumbImage}" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_only_ThumbColor_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider ThumbColor="Red" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_only_ThumbImageSource_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider ThumbImageSource="thumb.png" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_neither_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider Minimum="0" Maximum="100" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_ThumbColor_attribute_and_ThumbImageSource_property_element_is_flagged()
    {
        // The conflict fires regardless of declaration form; a property-element
        // ThumbImageSource still wins over an attribute ThumbColor at runtime.
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider [|ThumbColor="Red"|]>
                    <Slider.ThumbImageSource>thumb.png</Slider.ThumbImageSource>
                </Slider>
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_with_both_thumb_properties_as_property_elements_is_flagged()
    {
        // Span falls on the <Slider.ThumbColor> opening-tag name since ThumbColor is the
        // trigger and carries the diagnostic under both forms.
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider>
                    <[|Slider.ThumbColor|]>Red</Slider.ThumbColor>
                    <Slider.ThumbImageSource>thumb.png</Slider.ThumbImageSource>
                </Slider>
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Slider_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only. Dialect gating excludes WPF.
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Slider ThumbColor="Red" ThumbImageSource="thumb.png" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX506 -->
                <Slider ThumbColor="Red" ThumbImageSource="thumb.png" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_offending_sliders_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX506_SliderThumbColorAndImageConflict>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Slider [|ThumbColor="Red"|] ThumbImageSource="thumb.png" />
                <Slider [|ThumbColor="Blue"|] ThumbImageSource="thumb2.png" />
            </StackLayout>
            """,
            Dialect.Maui);
    }
}
