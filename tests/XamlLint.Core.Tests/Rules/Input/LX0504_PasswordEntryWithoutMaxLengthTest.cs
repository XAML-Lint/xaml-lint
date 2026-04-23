using XamlLint.Core.Rules.Input;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Input;

public sealed class LX0504_PasswordEntryWithoutMaxLengthTest
{
    private const string MauiXmlns = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Password_entry_without_MaxLength_is_flagged()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry [|IsPassword="True"|] />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_with_literal_MaxLength_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry IsPassword="True" MaxLength="64" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_with_MaxLength_zero_is_not_flagged()
    {
        // Literal zero is still presence — it may mean "unlimited" per the framework, but the
        // author has at least acknowledged the attribute.
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry IsPassword="True" MaxLength="0" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_with_bound_MaxLength_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            """
            <StackLayout xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Entry IsPassword="True" MaxLength="{Binding Max}" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_with_IsPassword_lowercase_true_is_flagged()
    {
        // Case-insensitive match: "true" must also fire.
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry [|IsPassword="true"|] />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_IsPassword_False_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry IsPassword="False" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_without_IsPassword_attribute_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_bound_IsPassword_is_not_flagged()
    {
        // A bound IsPassword defers to runtime — we can't know statically whether it's a
        // password field, so we don't flag it.
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            """
            <StackLayout xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
                <Entry IsPassword="{Binding Secret}" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_with_IsPassword_property_element_and_no_MaxLength_is_flagged()
    {
        // Property-element syntax must trigger the rule just like the attribute form; the
        // diagnostic span falls on the <Entry.IsPassword> opening-tag name, where the
        // trigger lives.
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry>
                    <[|Entry.IsPassword|]>True</Entry.IsPassword>
                </Entry>
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_with_MaxLength_property_element_is_not_flagged()
    {
        // The suppressor check must also see property-element form; MaxLength-in-element-form
        // is the author saying "I've set a length cap" just as clearly as the attribute form.
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry IsPassword="True">
                    <Entry.MaxLength>32</Entry.MaxLength>
                </Entry>
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Password_entry_on_Wpf_dialect_is_not_flagged()
    {
        // The rule is MAUI-only; dialect gating must exclude WPF.
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Entry IsPassword="True" />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <!-- xaml-lint disable once LX0504 -->
                <Entry IsPassword="True" />
            </StackLayout>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Multiple_password_entries_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0504_PasswordEntryWithoutMaxLength>.Analyze(
            $"""
            <StackLayout xmlns="{MauiXmlns}">
                <Entry [|IsPassword="True"|] />
                <Entry [|IsPassword="True"|] />
            </StackLayout>
            """,
            Dialect.Maui);
    }
}
