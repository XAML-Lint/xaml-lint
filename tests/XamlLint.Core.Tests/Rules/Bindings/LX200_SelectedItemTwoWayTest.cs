using XamlLint.Core.Rules.Bindings;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Bindings;

public sealed class LX200_SelectedItemTwoWayTest
{
    [Fact]
    public void Binding_without_mode_is_flagged()
    {
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      [|SelectedItem="{Binding Current}"|] />
            """);
    }

    [Fact]
    public void Binding_with_mode_twoway_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      SelectedItem="{Binding Current, Mode=TwoWay}" />
            """);
    }

    [Fact]
    public void Binding_with_mode_oneway_is_flagged()
    {
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      [|SelectedItem="{Binding Current, Mode=OneWay}"|] />
            """);
    }

    [Fact]
    public void XBind_without_mode_is_flagged()
    {
        // x:Bind default mode is OneTime, which is never correct for SelectedItem.
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      [|SelectedItem="{x:Bind Current}"|] />
            """);
    }

    [Fact]
    public void XBind_with_mode_twoway_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      SelectedItem="{x:Bind Current, Mode=TwoWay}" />
            """);
    }

    [Fact]
    public void Literal_SelectedItem_is_ignored()
    {
        // Non-binding values are irrelevant to this rule — some tests/designers use a
        // placeholder string.
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      SelectedItem="placeholder" />
            """);
    }

    [Fact]
    public void Non_binding_extension_is_ignored()
    {
        // StaticResource and other non-binding extensions are not checked.
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      SelectedItem="{StaticResource DefaultItem}" />
            """);
    }

    [Fact]
    public void Converter_with_nested_braces_does_not_confuse_parser()
    {
        XamlDiagnosticVerifier<LX200_SelectedItemTwoWay>.Analyze(
            """
            <ListView xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      SelectedItem="{Binding Current, Converter={StaticResource C}, Mode=TwoWay}" />
            """);
    }
}
