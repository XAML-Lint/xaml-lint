using XamlLint.Core.Rules.Bindings;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Bindings;

public sealed class LX0201_PreferXBindTest
{
    [Fact]
    public void Binding_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label}"|] />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Binding_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label}"|] />
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void Binding_on_Uno_is_flagged()
    {
        // Uno Platform supports {x:Bind} via its WinUI-compatible compiler; the "prefer x:Bind"
        // guidance applies identically on Uno targets.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label}"|] />
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Binding_on_Wpf_is_not_flagged()
    {
        // LX0201 targets UWP/WinUI 3/Uno only; the dispatcher's Dialects-mask gate filters WPF out
        // before the rule even runs, so no diagnostic is emitted.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{Binding Label}" />
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Binding_on_Maui_is_not_flagged()
    {
        // MAUI has no {x:Bind}; the rule is deliberately scoped away from it.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{Binding Label}" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Binding_on_Avalonia_is_not_flagged()
    {
        // Avalonia has no {x:Bind}; the rule is deliberately scoped away from it.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{Binding Label}" />
            """,
            Dialect.Avalonia);
    }

    [Fact]
    public void XBind_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    Content="{x:Bind Label}" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void StaticResource_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{StaticResource ButtonLabel}" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TemplateBinding_is_not_flagged()
    {
        // TemplateBinding is already the ControlTemplate-optimal form; x:Bind is not meant to
        // replace it.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="{TemplateBinding Label}" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Literal_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="Hello" />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_bindings_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Content="{Binding First}"|] />
                <Button [|Content="{Binding Second}"|] />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Binding_with_nested_converter_braces_is_still_flagged()
    {
        // The nested {StaticResource C} must not confuse the outer-extension detector.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding Label, Converter={StaticResource C}}"|] />
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Empty_binding_shorthand_is_flagged()
    {
        // `{Binding}` with no path binds to the current DataContext. Still a {Binding} usage,
        // so still subject to the "prefer x:Bind" recommendation.
        XamlDiagnosticVerifier<LX0201_PreferXBind>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="{Binding}"|] />
            """,
            Dialect.WinUI3);
    }
}
