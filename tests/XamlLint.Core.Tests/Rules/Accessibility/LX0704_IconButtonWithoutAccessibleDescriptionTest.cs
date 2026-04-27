using XamlLint.Core.Rules.Accessibility;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Accessibility;

public sealed class LX0704_IconButtonWithoutAccessibleDescriptionTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string WinUi = "http://schemas.microsoft.com/winui/2021/xaml";

    // ===== Symbol-content cases (Content="..." attribute) =====

    [Fact]
    public void Button_with_single_dash_Content_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}" Content="-" />
            """);
    }

    [Fact]
    public void Button_with_single_plus_Content_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}" Content="+" />
            """);
    }

    [Fact]
    public void Button_with_PUA_glyph_Content_is_flagged()
    {
        // Segoe MDL2 Assets glyph — Unicode PUA, not a letter/digit.
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}" Content="&#xE10A;" />
            """);
    }

    [Fact]
    public void Button_with_text_Content_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}" Content="OK" />
            """);
    }

    [Fact]
    public void Button_with_digit_Content_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}" Content="1" />
            """);
    }

    // ===== Icon-element-child cases =====

    [Fact]
    public void Button_with_only_Image_child_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}">
                <Image Source="save.png" />
            </Button>
            """);
    }

    [Fact]
    public void Button_with_only_Path_child_is_flagged()
    {
        // Canonical WPF icon idiom.
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}">
                <Path Data="M0,0 L10,10" Fill="Black" />
            </Button>
            """);
    }

    [Fact]
    public void Button_with_only_FontIcon_child_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{WinUi}}">
                <FontIcon Glyph="&#xE10A;" />
            </Button>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Button_with_only_SymbolIcon_child_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{WinUi}}">
                <SymbolIcon Symbol="Add" />
            </Button>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Button_with_TextBlock_child_is_not_flagged()
    {
        // The child is text-bearing — not an icon-only button.
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}">
                <TextBlock Text="Save" />
            </Button>
            """);
    }

    [Fact]
    public void Button_with_StackPanel_child_is_not_flagged()
    {
        // Multi-element / complex content is not the rule's concern (out of scope; documented).
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}">
                <StackPanel>
                    <Image Source="save.png" />
                    <TextBlock Text="Save" />
                </StackPanel>
            </Button>
            """);
    }

    // ===== Empty button =====

    [Fact]
    public void Empty_Button_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}" />
            """);
    }

    // ===== Suppressors =====

    [Fact]
    public void Icon_only_Button_with_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}" Content="-" AutomationProperties.Name="Decrease" />
            """);
    }

    [Fact]
    public void Icon_only_Button_with_AutomationProperties_HelpText_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}" Content="-" AutomationProperties.HelpText="Decrease quantity" />
            """);
    }

    [Fact]
    public void Icon_only_Button_with_AutomationId_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}" Content="-" AutomationId="DecreaseButton" />
            """);
    }

    [Fact]
    public void Icon_only_Button_with_AutomationProperties_LabeledBy_resolved_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2009}}">
                <Label x:Name="DecLabel" Content="Decrease" />
                <Button Content="-" AutomationProperties.LabeledBy="{x:Reference DecLabel}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Icon_only_Button_with_AutomationProperties_LabeledBy_dangling_is_flagged()
    {
        // Same posture as LX0700/0701/0702/0703: dangling LabeledBy does NOT suppress.
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2009}}">
                <[|Button|] Content="-" AutomationProperties.LabeledBy="{x:Reference Ghost}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Icon_only_Button_with_IsInAccessibleTree_False_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <Button xmlns="{{Wpf}}" Content="-" AutomationProperties.IsInAccessibleTree="False" />
            """);
    }

    [Fact]
    public void Icon_only_Button_with_IsInAccessibleTree_True_is_still_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|Button|] xmlns="{{Wpf}}" Content="-" AutomationProperties.IsInAccessibleTree="True" />
            """);
    }

    [Fact]
    public void Icon_only_MAUI_ImageButton_with_SemanticProperties_Description_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <ImageButton xmlns="{{Maui}}" Source="save.png" SemanticProperties.Description="Save document" />
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Icon_only_MAUI_ImageButton_without_a11y_is_flagged()
    {
        XamlDiagnosticVerifier<LX0704_IconButtonWithoutAccessibleDescription>.Analyze(
            $$"""
            <[|ImageButton|] xmlns="{{Maui}}" Source="save.png" />
            """,
            Dialect.Maui);
    }
}
