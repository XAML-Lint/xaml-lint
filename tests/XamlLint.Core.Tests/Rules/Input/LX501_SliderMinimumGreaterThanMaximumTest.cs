using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX501_SliderMinimumGreaterThanMaximumTest
{
    [Fact]
    public void Minimum_greater_than_Maximum_on_Wpf_is_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Minimum="10"|] Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Minimum_greater_than_Maximum_on_Maui_is_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    [|Minimum="10"|] Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Minimum_greater_than_Maximum_on_WinUI3_is_not_flagged()
    {
        // LX501's dialects are Wpf + Maui only — UWP/WinUI raise a runtime exception on this
        // state, so static analysis is redundant there.
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="10" Maximum="5" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Minimum_equals_Maximum_is_not_flagged()
    {
        // A single-valued range is legal (degenerate but not an error).
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="5" Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Minimum_less_than_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="0" Maximum="100" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Only_Minimum_attribute_present_is_not_flagged()
    {
        // Missing Maximum means the framework default (typically 1.0) applies — unknown at
        // lint time, so stay quiet.
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="10" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Bound_Minimum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="{Binding Low}" Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Bound_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Minimum="10" Maximum="{Binding High}" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Decimal_Minimum_greater_than_Maximum_is_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Minimum="2.5"|] Maximum="1.5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_Slider_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX501_SliderMinimumGreaterThanMaximum>.Analyze(
            """
            <ProgressBar xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                         Minimum="10" Maximum="5" />
            """,
            Dialect.Wpf);
    }
}
