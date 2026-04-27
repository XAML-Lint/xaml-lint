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

    [Fact]
    public void Grid_with_Visibility_attribute_is_not_flagged()
    {
        // The "lazy multibinding" pattern: outer Grid carries Visibility, child carries its own.
        // Removing the wrapper would lose the outer Visibility hookup.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" Visibility="{Binding ShowOuter}">
                <Border Visibility="{Binding ShowInner}" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_Background_attribute_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" Background="White">
                <ContentPresenter />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_Margin_attribute_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" Margin="20">
                <ContentPresenter />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_xName_attribute_is_not_flagged()
    {
        // x:Name suggests code-behind reach; we can't see C# so we trust the author.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" xmlns:x="{{Xaml2009}}" x:Name="MainContainer">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_Style_attribute_is_not_flagged()
    {
        // A Style might set RowDefinitions or any other property; trust the author.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" Style="{StaticResource GridStyle}">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_attached_property_assignment_is_not_flagged()
    {
        // Grid is positioned inside a parent Grid via Grid.Row attached. Removing it would
        // need to copy Grid.Row onto the child.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" Grid.Row="0">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_event_handler_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" MouseDown="OnMouseDown">
                <Button Content="only" />
            </Grid>
            """);
    }

    [Fact]
    public void Grid_with_only_xmlns_declarations_is_flagged()
    {
        // Pure xmlns is the "bare Grid" case — still flagged.
        XamlDiagnosticVerifier<LX0106_SingleChildGridWithoutDefinitions>.Analyze(
            $$"""
            <[|Grid|] xmlns="{{Wpf}}" xmlns:local="clr-namespace:Sample">
                <local:CustomControl />
            </Grid>
            """);
    }
}
