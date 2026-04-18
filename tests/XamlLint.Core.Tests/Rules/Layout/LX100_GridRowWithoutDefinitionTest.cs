using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX100_GridRowWithoutDefinitionTest
{
    [Fact]
    public void Grid_Row_out_of_range_is_flagged()
    {
        // No RowDefinitions means the Grid has one implicit row (index 0).
        // Grid.Row="1" targets a row that does not exist.
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.Row="1"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_Row_zero_with_implicit_single_row_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.Row="0" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_Row_in_range_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Button Grid.Row="1" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_Row_equal_to_row_count_is_flagged()
    {
        // 2 RowDefinitions → valid row indexes are 0 and 1. Grid.Row="2" is one past the end.
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Button [|Grid.Row="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Element_outside_any_Grid_is_not_flagged()
    {
        // Grid.Row on a StackPanel child has no runtime effect without a Grid ancestor —
        // noisy to flag, and the user almost certainly copy-pasted it. Silently skip.
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.Row="5" />
            </StackPanel>
            """);
    }

    [Fact]
    public void WinUI_shorthand_RowDefinitions_attribute_is_respected()
    {
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  RowDefinitions="Auto,*,Auto">
                <Button Grid.Row="2" />
                <Button [|Grid.Row="3"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Non_integer_Grid_Row_is_ignored()
    {
        // A markup-extension value like {Binding Idx} is resolved at runtime; we do not have
        // enough information to validate it statically, so the rule stays quiet.
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button Grid.Row="{Binding Idx}" />
            </Grid>
            """);
    }

    [Fact]
    public void Nested_Grid_child_uses_inner_Grid_row_count()
    {
        // The outer Grid has 3 rows; the inner Grid has 1 implicit row. The inner Button's
        // Grid.Row="2" is out of range for the inner Grid, not the outer.
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Grid Grid.Row="0">
                    <Button [|Grid.Row="2"|] />
                </Grid>
            </Grid>
            """);
    }

    [Fact]
    public void Element_syntax_Grid_Row_is_flagged()
    {
        // Element syntax is uncommon but legal. The harness's {|LXnnn|} bare marker asserts
        // that a diagnostic exists for this rule somewhere in the document; precise span
        // assertion for element syntax would require a nested-marker form the harness does
        // not yet support (see docs/superpowers/specs/2026-04-17-xaml-lint-design.md §8.2).
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            {|LX100|}<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button>
                    <Grid.Row>1</Grid.Row>
                </Button>
            </Grid>
            """);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX100_GridRowWithoutDefinition>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.Row="1"|] />
                <Button [|Grid.Row="2"|] />
            </Grid>
            """);
    }
}
