using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX0104_GridDefinitionShorthandUnsupportedTest
{
    [Fact]
    public void Wpf_legacy_RowDefinitions_shorthand_is_flagged()
    {
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  [|RowDefinitions="Auto,*"|] />
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }

    [Fact]
    public void Wpf_legacy_ColumnDefinitions_shorthand_is_flagged()
    {
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  [|ColumnDefinitions="*,Auto"|] />
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }

    [Fact]
    public void Wpf_legacy_both_shorthand_attributes_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  [|RowDefinitions="Auto,*"|] [|ColumnDefinitions="*,Auto"|] />
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }

    [Fact]
    public void Wpf_modern_framework_does_not_flag_shorthand()
    {
        // WPF on .NET 10+ supports the shorthand; nothing to flag.
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  RowDefinitions="Auto,*"
                  ColumnDefinitions="*,Auto" />
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 10);
    }

    [Fact]
    public void Wpf_with_unspecified_framework_does_not_flag_shorthand()
    {
        // No frameworkVersion set → assume newest → shorthand is supported.
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  RowDefinitions="Auto,*" />
            """,
            dialect: Dialect.Wpf);
    }

    [Fact]
    public void WinUI3_with_any_framework_does_not_flag_shorthand()
    {
        // Non-WPF dialects support the shorthand natively regardless of framework version.
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  RowDefinitions="Auto,*" />
            """,
            dialect: Dialect.WinUI3,
            frameworkMajorVersion: 9);
    }

    [Fact]
    public void Wpf_legacy_with_only_element_syntax_RowDefinitions_is_not_flagged()
    {
        // Element syntax works on every WPF version. Only the shorthand attribute is the issue.
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
            </Grid>
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }

    [Fact]
    public void Wpf_legacy_shorthand_on_non_Grid_element_is_not_flagged()
    {
        // Only attributes on actual <Grid> elements are in scope. A user-defined element
        // happening to have a "RowDefinitions" attribute is irrelevant.
        XamlDiagnosticVerifier<LX0104_GridDefinitionShorthandUnsupported>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                        xmlns:custom="clr-namespace:My.Custom">
                <custom:Foo RowDefinitions="anything" />
            </StackPanel>
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }
}
