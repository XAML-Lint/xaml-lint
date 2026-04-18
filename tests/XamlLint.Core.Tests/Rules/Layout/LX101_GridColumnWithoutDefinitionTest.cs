using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX101_GridColumnWithoutDefinitionTest
{
    [Fact]
    public void Grid_Column_out_of_range_is_flagged()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.Column="1"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_Column_zero_with_implicit_single_column_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.Column="0" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_Column_in_range_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Button Grid.Column="1" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_Column_equal_to_column_count_is_flagged()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Button [|Grid.Column="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Element_outside_any_Grid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.Column="5" />
            </StackPanel>
            """);
    }

    [Fact]
    public void WinUI_shorthand_ColumnDefinitions_attribute_is_respected()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  ColumnDefinitions="*,Auto">
                <Button Grid.Column="1" />
                <Button [|Grid.Column="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Non_integer_Grid_Column_is_ignored()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button Grid.Column="{Binding Col}" />
            </Grid>
            """);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.Column="1"|] />
                <Button [|Grid.Column="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Nested_Grid_child_uses_inner_Grid_column_count()
    {
        // The outer Grid has 3 columns; the inner Grid has 1 implicit column. The inner Button's
        // Grid.Column="2" is out of range for the inner Grid, not the outer.
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Grid Grid.Column="0">
                    <Button [|Grid.Column="2"|] />
                </Grid>
            </Grid>
            """);
    }

    [Fact]
    public void Element_syntax_Grid_Column_is_flagged()
    {
        // Element syntax is uncommon but legal. The bare {|LX101|} marker asserts that a
        // diagnostic fires for this rule somewhere in the document.
        XamlDiagnosticVerifier<LX101_GridColumnWithoutDefinition>.Analyze(
            """
            {|LX101|}<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button>
                    <Grid.Column>1</Grid.Column>
                </Button>
            </Grid>
            """);
    }
}
