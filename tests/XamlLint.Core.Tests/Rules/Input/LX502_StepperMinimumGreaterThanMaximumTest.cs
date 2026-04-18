using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX502_StepperMinimumGreaterThanMaximumTest
{
    [Fact]
    public void Minimum_greater_than_Maximum_on_Maui_is_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     [|Minimum="10"|] Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Minimum_greater_than_Maximum_on_Wpf_is_not_flagged()
    {
        // Stepper is a MAUI-only control; on WPF the element name means nothing framework-wise
        // and the rule's Dialects mask filters the dispatcher call out entirely.
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     Minimum="10" Maximum="5" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Minimum_less_than_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="0" Maximum="100" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Minimum_equals_Maximum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="5" Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Bound_Minimum_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="{Binding Low}" Maximum="5" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Only_Minimum_attribute_present_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Stepper xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     Minimum="10" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Non_Stepper_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX502_StepperMinimumGreaterThanMaximum>.Analyze(
            """
            <Slider xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    Minimum="10" Maximum="5" />
            """,
            Dialect.Maui);
    }
}
