using XamlLint.Core.Rules.Naming;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Naming;

public sealed class LX0300_XNameCasingTest
{
    [Fact]
    public void Lowercase_x_Name_is_flagged()
    {
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Name="myButton"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Uppercase_x_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Name="MyButton" />
            </Grid>
            """);
    }

    [Fact]
    public void Underscore_prefix_is_flagged()
    {
        // Convention for "private" named elements varies; this rule enforces the canonical
        // WPF style: first character must be an uppercase letter (Unicode-aware, via char.IsUpper).
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Name="_hidden"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Trailing_digit_is_not_flagged()
    {
        // Digits are allowed anywhere except as the first character. The rule only inspects
        // value[0]; trailing digits (MyButton1) are conventional PascalCase and pass.
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Name="MyButton1" />
            </Grid>
            """);
    }

    [Fact]
    public void Digit_prefix_is_flagged()
    {
        // XAML identifiers forbid leading digits (WPF will reject at runtime and LX0001 may
        // fire on the parser side), but if the raw XML parser accepts the attribute value we
        // still catch the casing violation deterministically.
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Name="1stButton"|] />
            </Grid>
            """);
    }

    [Fact]
    public void Name_attribute_without_x_prefix_is_ignored()
    {
        // Only x:Name (the XAML 2006/2009 ns) is checked. An unprefixed Name= is a WPF
        // convenience that maps to the framework's NameProperty — still identifier-like, but
        // out of scope for this rule.
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button Name="lowercase" />
            </Grid>
            """);
    }

    [Fact]
    public void Empty_x_Name_is_ignored()
    {
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Name="" />
            </Grid>
            """);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0300_XNameCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Name="one"|] />
                <Button [|x:Name="two"|] />
            </Grid>
            """);
    }
}
