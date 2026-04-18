using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX103_GridColumnSpanExceedsColumnsTest
{
    [Fact]
    public void ColumnSpan_greater_than_column_count_is_flagged()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Button [|Grid.ColumnSpan="3"|] />
            </Grid>
            """);
    }

    [Fact]
    public void ColumnSpan_equal_to_column_count_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Button Grid.ColumnSpan="2" />
            </Grid>
            """);
    }

    [Fact]
    public void ColumnSpan_one_with_implicit_single_column_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.ColumnSpan="1" />
            </Grid>
            """);
    }

    [Fact]
    public void ColumnSpan_two_with_implicit_single_column_is_flagged()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.ColumnSpan="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Element_outside_any_Grid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.ColumnSpan="99" />
            </StackPanel>
            """);
    }

    [Fact]
    public void WinUI_shorthand_ColumnDefinitions_attribute_is_respected()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  ColumnDefinitions="*,Auto">
                <Button Grid.ColumnSpan="2" />
                <Button [|Grid.ColumnSpan="3"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Non_integer_Grid_ColumnSpan_is_ignored()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button Grid.ColumnSpan="{Binding Span}" />
            </Grid>
            """);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.ColumnSpan="2"|] />
                <Button [|Grid.ColumnSpan="3"|] />
            </Grid>
            """);
    }
}
