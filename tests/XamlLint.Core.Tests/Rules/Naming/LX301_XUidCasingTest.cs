using XamlLint.Core.Rules.Naming;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Naming;

public sealed class LX301_XUidCasingTest
{
    [Fact]
    public void Lowercase_x_Uid_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="loginButton"|] />
            </Grid>
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void Lowercase_x_Uid_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="loginButton"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Lowercase_x_Uid_on_Wpf_is_not_flagged()
    {
        // x:Uid's casing convention is a UWP/WinUI .resw concern; the rule's Dialects mask filters WPF out.
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="loginButton" />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Uppercase_x_Uid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="LoginButton" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Uid_attribute_without_x_prefix_is_ignored()
    {
        // Only x:Uid (the XAML 2006/2009 ns) is checked; unprefixed Uid has no framework
        // meaning on UWP/WinUI 3.
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Button Uid="lowercase" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Empty_x_Uid_is_ignored()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Digit_prefix_is_flagged()
    {
        // Digits aren't uppercase letters; the rule catches non-letter first-character
        // violations the same way it catches lowercase letters. Mirrors LX300's digit case.
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="1Button"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_violations_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="one"|] />
                <Button [|x:Uid="two"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }
}
