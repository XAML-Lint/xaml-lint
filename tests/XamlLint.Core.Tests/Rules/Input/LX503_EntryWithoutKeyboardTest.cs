using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX503_EntryWithoutKeyboardTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Entry_without_Keyboard_on_Maui_is_flagged()
    {
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <[|Entry|] />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_Keyboard_literal_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry Keyboard="Numeric" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_Keyboard_binding_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            """
            <StackLayout xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Entry Keyboard="{Binding Kbd}" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_empty_Keyboard_is_not_flagged()
    {
        // Presence of any Keyboard= attribute, even empty, is an "I considered this" signal.
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry Keyboard="" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_Keyboard_property_element_syntax_is_not_flagged()
    {
        // Upstream Rapid XAML Toolkit RXT300 flattens attribute and property-element syntax
        // via RapidXamlElement.HasAttribute; our detector does the same.
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry>
                    <Entry.Keyboard>Numeric</Entry.Keyboard>
                </Entry>
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only. A <Entry> in a WPF-namespaced doc doesn't exist as a
        // WPF control; still, the rule must not fire because dialect gating excludes WPF.
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Entry />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_Entry_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Label Text="hello" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX503 -->
                <Entry />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_offending_entries_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX503_EntryWithoutKeyboard>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <[|Entry|] />
                <[|Entry|] />
            </StackLayout>
            """,
            Dialect.Maui);
    }
}
