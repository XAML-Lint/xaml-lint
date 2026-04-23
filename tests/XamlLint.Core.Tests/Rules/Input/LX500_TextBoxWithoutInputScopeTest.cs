using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX500_TextBoxWithoutInputScopeTest
{
    [Fact]
    public void TextBox_without_InputScope_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
            </Grid>
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Uno_is_flagged()
    {
        // Uno fully implements InputScopeNameValue across all platform heads (Android, iOS,
        // WebAssembly, macOS, Skia, Windows) — the soft-keyboard hint applies identically.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
            </Grid>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Wpf_is_not_flagged()
    {
        // InputScope is a UWP/WinUI/Uno concept — meaningless on WPF.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Maui_is_not_flagged()
    {
        // MAUI uses Entry.Keyboard (LX503) rather than TextBox.InputScope.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox />
            </Grid>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void TextBox_without_InputScope_on_Avalonia_is_not_flagged()
    {
        // Avalonia does not surface InputScope; the rule's dialect mask filters it out.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox />
            </Grid>
            """,
            Dialect.Avalonia);
    }

    [Fact]
    public void TextBox_with_InputScope_attribute_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox InputScope="Number" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_with_InputScope_binding_is_not_flagged()
    {
        // A bound InputScope is still an "I know what I'm doing" signal; don't second-guess it.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox InputScope="{Binding Scope}" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_with_empty_InputScope_is_not_flagged()
    {
        // Authored intent: any InputScope attribute presence (including empty value) signals
        // "I've considered this". The rule only flags absence.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox InputScope="" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBox_with_InputScope_property_element_syntax_is_not_flagged()
    {
        // Property-element syntax is semantically equivalent to the attribute form; the rule
        // must suppress on both.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBox>
                    <TextBox.InputScope>Number</TextBox.InputScope>
                </TextBox>
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void TextBlock_is_not_flagged()
    {
        // TextBlock is not an input control.
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock Text="read-only" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX500_TextBoxWithoutInputScope>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|TextBox|] />
                <[|TextBox|] />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }
}
