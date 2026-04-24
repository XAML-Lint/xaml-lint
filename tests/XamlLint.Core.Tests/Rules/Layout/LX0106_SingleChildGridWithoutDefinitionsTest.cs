using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX0106_SingleChildGridWithoutDefinitionsTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";

    [Fact]
    public void Grid_with_single_child_and_no_definitions_is_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_two_children_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Button Content="one" />
                <Button Content="two" />
            </Grid>
            """);
    }

    [Fact]
    public void Empty_Grid_is_not_flagged()
    {
        // Zero children is a different signal — placeholder, data-bound, etc.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" />
            """);
    }

    [Fact]
    public void Grid_with_RowDefinitions_shorthand_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" RowDefinitions="Auto,*">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_ColumnDefinitions_shorthand_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" ColumnDefinitions="*,Auto">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_element_syntax_RowDefinitions_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_element_syntax_ColumnDefinitions_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition />
                </Grid.ColumnDefinitions>
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_empty_RowDefinitions_element_is_flagged()
    {
        // <Grid.RowDefinitions/> with no <RowDefinition> children behaves like no declaration
        // at all — matches CountRowDefinitions's fall-through to 1 implicit row.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}">
                <Grid.RowDefinitions />
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_only_Grid_Resources_property_element_and_one_child_is_flagged()
    {
        // <Grid.Resources> is a property-element node, not a layout child. Still one
        // layout child → redundant wrapper.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}">
                <Grid.Resources>
                    <SolidColorBrush x:Key="Foo" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" Color="Red" />
                </Grid.Resources>
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Named_Grid_with_single_child_is_flagged()
    {
        // x:Name / Name on the Grid does not suppress — the Grid is still structurally
        // redundant.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" x:Name="Root">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Styled_Grid_with_single_child_is_flagged()
    {
        // LX0106 is local and structural; it cannot reason about whether a Style supplies
        // definitions. Noise like this is exactly why the rule is off in :recommended.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}" Style="{StaticResource FooGrid}">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Nested_redundant_Grid_is_flagged_once_per_grid()
    {
        // A redundant Grid nested inside another redundant Grid is two separate problems.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}">
                <[|Grid|]>
                    <Button Content="deep" />
                </Grid>
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_single_child_inside_Grid_with_definitions_is_flagged()
    {
        // Inner Grid is redundant even though outer Grid is fine. Only the inner Grid
        // name span is marked — the outer carries RowDefinitions and is not flagged.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" RowDefinitions="Auto,*">
                <Button Grid.Row="0" Content="header" />
                <[|Grid|] Grid.Row="1">
                    <TextBlock Text="body" />
                </Grid>
            </Grid>
            """);
    }

    [Fact]
    public void Maui_Grid_with_single_child_is_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <[|Grid|]>
                    <Label Text="only" />
                </Grid>
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Wpf_legacy_framework_with_RowDefinitions_shorthand_is_not_flagged()
    {
        // LX0106 is a presence check; it does NOT require that the shorthand be supported
        // by the dialect+framework combo (that's LX0104's job). The author clearly intended
        // multi-row layout — do not double-flag them.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" RowDefinitions="Auto,*">
                <Button Content="only" />
            </Grid>
            """,
            dialect: Dialect.Wpf,
            frameworkMajorVersion: 9);
    }

    [Fact]
    public void Diagnostic_span_covers_the_Grid_element_name()
    {
        // Sanity check: the marker is exactly "Grid" inside the opening tag — matches
        // LocationHelpers.GetElementNameSpan.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}">
                <Button Content="only" />
            </Grid>
            """);
    }
}
