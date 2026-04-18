using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX102_GridRowSpanExceedsRowsTest
{
    [Fact]
    public void RowSpan_greater_than_row_count_is_flagged()
    {
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
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
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
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
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.RowSpan="1" />
            </Grid>
            """);
    }

    [Fact]
    public void RowSpan_two_with_implicit_single_row_is_flagged()
    {
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.RowSpan="2"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Element_outside_any_Grid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Grid.RowSpan="99" />
            </StackPanel>
            """);
    }

    [Fact]
    public void WinUI_shorthand_RowDefinitions_attribute_is_respected()
    {
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
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
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
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
        XamlDiagnosticVerifier<LX102_GridRowSpanExceedsRows>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button [|Grid.RowSpan="2"|] />
                <Button [|Grid.RowSpan="3"|] />
            </Grid>
            """);
    }
}
