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
    public void Grid_definition_shorthand_ColumnDefinitions_attribute_is_respected()
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

    [Fact]
    public void Nested_Grid_child_uses_inner_Grid_column_count()
    {
        // The outer Grid has 3 columns; the inner Grid has 1 implicit column. The inner
        // Button's Grid.ColumnSpan="2" exceeds the inner Grid's single column, not the
        // outer's three.
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Grid Grid.Column="0">
                    <Button [|Grid.ColumnSpan="2"|] />
                </Grid>
            </Grid>
            """);
    }

    [Fact]
    public void Element_syntax_Grid_ColumnSpan_is_flagged()
    {
        // Element syntax is uncommon but legal. The bare {|LX103|} marker asserts that a
        // diagnostic fires for this rule somewhere in the document.
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            {|LX103|}<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button>
                    <Grid.ColumnSpan>2</Grid.ColumnSpan>
                </Button>
            </Grid>
            """);
    }

    [Fact]
    public void Wpf_legacy_framework_ignores_shorthand_when_evaluating_ColumnSpan()
    {
        // On WPF .NET 9, ColumnDefinitions="Auto,*" doesn't take effect — the Grid has 1
        // implicit column, so Grid.ColumnSpan="2" exceeds it.
        XamlDiagnosticVerifier<LX103_GridColumnSpanExceedsColumns>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  ColumnDefinitions="Auto,*">
                <Button [|Grid.ColumnSpan="2"|] />
            </Grid>
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }
}
