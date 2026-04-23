using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX0102_GridRowSpanExceedsRowsTest
{
    [Fact]
    public void RowSpan_greater_than_row_count_is_flagged()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Button [|Grid.RowSpan="3"|] />
            </Grid>
            """);
    }

    [Fact]
    public void RowSpan_equal_to_row_count_is_not_flagged()
    {
        // An element with RowSpan matching the total row count spans the whole Grid and is
        // legal — the common "header across all rows" idiom.
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Button Grid.RowSpan="2" />
            </Grid>
            """);
    }

    [Fact]
    public void RowSpan_one_with_implicit_single_row_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.RowSpan="1" />
            </Grid>
            """);
    }

    [Fact]
    public void RowSpan_two_with_implicit_single_row_is_flagged()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.RowSpan="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Element_outside_any_Grid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.RowSpan="99" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Grid_definition_shorthand_RowDefinitions_attribute_is_respected()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  RowDefinitions="Auto,*,Auto">
                <Button Grid.RowSpan="3" />
                <Button [|Grid.RowSpan="4"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Non_integer_Grid_RowSpan_is_ignored()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button Grid.RowSpan="{Binding Span}" />
            </Grid>
            """);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.RowSpan="2"|] />
                <Button [|Grid.RowSpan="3"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Nested_Grid_child_uses_inner_Grid_row_count()
    {
        // The outer Grid has 3 rows; the inner Grid has 1 implicit row. The inner Button's
        // Grid.RowSpan="2" exceeds the inner Grid's single row, not the outer's three.
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Grid Grid.Row="0">
                    <Button [|Grid.RowSpan="2"|] />
                </Grid>
            </Grid>
            """);
    }

    [Fact]
    public void Element_syntax_Grid_RowSpan_is_flagged()
    {
        // Element syntax is uncommon but legal. The bare {|LX0102|} marker asserts that a
        // diagnostic fires for this rule somewhere in the document.
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            {|LX0102|}<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button>
                    <Grid.RowSpan>2</Grid.RowSpan>
                </Button>
            </Grid>
            """);
    }

    [Fact]
    public void Wpf_legacy_framework_ignores_shorthand_when_evaluating_RowSpan()
    {
        // On WPF .NET 9, RowDefinitions="Auto,*" doesn't take effect — the Grid has 1 implicit
        // row, so Grid.RowSpan="2" exceeds it.
        XamlDiagnosticVerifier<LX0102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  RowDefinitions="Auto,*">
                <Button [|Grid.RowSpan="2"|] />
            </Grid>
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }
}
