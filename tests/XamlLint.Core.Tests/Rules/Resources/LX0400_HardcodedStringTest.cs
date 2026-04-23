using XamlLint.Core.Rules.Resources;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Resources;

public sealed class LX0400_HardcodedStringTest
{
    [Fact]
    public void Hardcoded_Text_is_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       [|Text="Click me"|] />
            """);
    }

    [Fact]
    public void Hardcoded_Title_is_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Title="Main Window"|] />
            """);
    }

    [Fact]
    public void Hardcoded_Content_on_Button_is_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="Save"|] />
            """);
    }

    [Fact]
    public void Binding_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="{Binding Greeting}" />
            """);
    }

    [Fact]
    public void StaticResource_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="{StaticResource GreetingText}" />
            """);
    }

    [Fact]
    public void Empty_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="" />
            """);
    }

    [Fact]
    public void Whitespace_only_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="   " />
            """);
    }

    [Fact]
    public void Attribute_not_in_scope_is_not_flagged()
    {
        // Width is not a text-presenting attribute.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="{Binding Hello}"
                       Width="100" />
            """);
    }

    [Fact]
    public void Multiple_hardcoded_strings_each_flag()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock [|Text="First"|] />
                <TextBlock [|Text="Second"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void PlaceholderText_is_flagged()
    {
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     [|PlaceholderText="Enter name"|] />
            """);
    }

    [Fact]
    public void Attribute_with_custom_namespace_is_ignored()
    {
        // Only unprefixed (empty-namespace) attributes are in scope. A Blend design-time
        // attribute like d:Text carries a non-empty namespace and is skipped — it's not a
        // user-facing string on a production control.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                       d:Text="design-time only" />
            """);
    }

    [Fact]
    public void Icon_font_glyph_in_Private_Use_Area_is_not_flagged()
    {
        // Segoe MDL2 Assets / Segoe Fluent Icons / Material Icons / FontAwesome all map
        // glyphs to the BMP Private Use Area (U+E000–U+F8FF). A bare icon reference like
        // &#xE711; is design content, not localisable prose.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    FontFamily="Segoe MDL2 Assets"
                    Content="&#xE711;" />
            """);
    }

    [Fact]
    public void Multiple_PUA_glyphs_with_whitespace_are_not_flagged()
    {
        // Internal whitespace between glyphs is still icon content, not prose.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       FontFamily="Segoe MDL2 Assets"
                       Text="&#xE711; &#xE712;" />
            """);
    }

    [Fact]
    public void Supplementary_PUA_codepoint_is_not_flagged()
    {
        // Supplementary Private Use Area-A starts at U+F0000 — encoded as the surrogate
        // pair 󰀀. Verifies the surrogate-pair path through the PUA check.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="&#xF0000;" />
            """);
    }

    [Fact]
    public void PUA_glyph_mixed_with_prose_is_flagged()
    {
        // A glyph prefix doesn't turn real prose into icon content.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    FontFamily="Segoe MDL2 Assets"
                    [|Content="&#xE711; Close"|] />
            """);
    }

    [Fact]
    public void Single_letter_is_flagged()
    {
        // A single real letter could be localisable copy (keyboard keys, game tiles,
        // tic-tac-toe squares). Only non-letter, non-digit characters are exempt.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="X"|] />
            """);
    }

    [Fact]
    public void Plus_symbol_is_not_flagged()
    {
        // Common zoom-in / expand / spin-button caption; not localisable prose.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="+" />
            """);
    }

    [Fact]
    public void Minus_symbol_is_not_flagged()
    {
        // Common zoom-out / collapse / spin-button caption; not localisable prose.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    Content="-" />
            """);
    }

    [Fact]
    public void Colon_separator_is_not_flagged()
    {
        // Trailing colon is ubiquitous label punctuation (rendered after a bound label).
        // Treat as chrome.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text=":" />
            """);
    }

    [Fact]
    public void Multi_char_pure_symbols_are_not_flagged()
    {
        // Pure-symbol sequences like "->", "<<", "..." are chrome, not copy.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Content="&lt;&lt;" />
                <Button Content="-&gt;" />
                <Button Content="..." />
            </StackPanel>
            """);
    }

    [Fact]
    public void Symbol_mixed_with_letters_is_flagged()
    {
        // A symbol prefix doesn't turn real prose into chrome.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="+ Add"|] />
            """);
    }

    [Fact]
    public void Single_digit_is_flagged()
    {
        // Digits localise: "1" becomes different glyphs in Arabic / Thai / Devanagari
        // scripts. The exemption is symbols and PUA glyphs only.
        XamlDiagnosticVerifier<LX0400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="1"|] />
            """);
    }
}
