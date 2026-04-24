using XamlLint.Core.Rules.Layout;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Layout;

public sealed class LX0105_ZeroSizedGridDefinitionTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";

    [Fact]
    public void RowDefinition_Height_zero_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="0"|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_decimal_zero_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="0.0"|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_negative_integer_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="-1"|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_negative_decimal_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="-0.5"|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_empty_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height=""|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_whitespace_only_is_flagged()
    {
        // Whitespace-only value is treated as empty after trimming.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="   "|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void ColumnDefinition_Width_zero_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition [|Width="0"|] />
                </Grid.ColumnDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void ColumnDefinition_Width_negative_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition [|Width="-5"|] />
                </Grid.ColumnDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void ColumnDefinition_Width_empty_is_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition [|Width=""|] />
                </Grid.ColumnDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_Auto_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_Auto_lowercase_is_not_flagged()
    {
        // GridLength parsing is case-insensitive on every dialect.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="auto" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_star_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*" />
                    <RowDefinition Height="2*" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_zero_star_is_not_flagged()
    {
        // 0* is a valid GridLength (zero weight). Deliberately not our concern — it is
        // a star-sized value, not a literal pixel value. See rule doc "What is out of scope".
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="0*" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_positive_integer_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="100" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_positive_fraction_is_not_flagged()
    {
        // 0.5 is a valid GridLength (half a device-independent pixel). Weird but legal.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="0.5" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_without_Height_is_not_flagged()
    {
        // Default is "*" — not zero-sized.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_markup_extension_is_not_flagged()
    {
        // Markup-extension values are not literal and cannot be reasoned about.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="{Binding RowHeight}" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_Height_unparseable_value_is_not_flagged()
    {
        // Unrecognized string — not a literal we can reason about. Other tooling can warn.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="banana" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void Multiple_definitions_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="0"|] />
                    <RowDefinition Height="*" />
                    <RowDefinition [|Height="-1"|] />
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition [|Width=""|] />
                </Grid.ColumnDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void Width_on_RowDefinition_is_ignored()
    {
        // LX0105 only reads Height on RowDefinition and Width on ColumnDefinition; a Width
        // on RowDefinition is meaningless and not LX0105's concern.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition Width="0" />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void Height_on_ColumnDefinition_is_ignored()
    {
        // Mirror of the above.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Height="0" />
                </Grid.ColumnDefinitions>
            </Grid>
            """);
    }

    [Fact]
    public void RowDefinition_outside_Grid_is_still_flagged()
    {
        // LX0105 is a literal-value check; it does not require a Grid ancestor. A
        // <RowDefinition Height="0"/> floating in markup is still structurally wrong.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <RowDefinition [|Height="0"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Maui_dialect_also_flags()
    {
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition [|Height="0"|] />
                    </Grid.RowDefinitions>
                </Grid>
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Diagnostic_span_covers_the_entire_attribute()
    {
        // Sanity check: the marker span includes the attribute name, '=', the opening and
        // closing quotes, and the value — exactly what LocationHelpers.GetAttributeSpan
        // returns.
        XamlDiagnosticVerifier<LX0105_ZeroSizedGridDefinition>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}">
                <Grid.RowDefinitions>
                    <RowDefinition [|Height="0"|] />
                </Grid.RowDefinitions>
            </Grid>
            """);
    }
}
