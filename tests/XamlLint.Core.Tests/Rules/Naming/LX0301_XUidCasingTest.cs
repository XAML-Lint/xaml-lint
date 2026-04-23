using XamlLint.Core.Rules.Naming;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Naming;

public sealed class LX0301_XUidCasingTest
{
    [Fact]
    public void Lowercase_x_Uid_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
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
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="loginButton"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Lowercase_x_Uid_on_Uno_is_flagged()
    {
        // Uno uses UWP-style x:Uid + .resw localization; the casing convention applies.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="loginButton"|] />
            </Grid>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Lowercase_x_Uid_on_Wpf_is_not_flagged()
    {
        // x:Uid's casing convention is a UWP/WinUI/Uno .resw concern; the rule's Dialects mask filters WPF out.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="loginButton" />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Lowercase_x_Uid_on_Maui_is_not_flagged()
    {
        // MAUI localization doesn't use .resw; x:Uid has no framework meaning there.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="loginButton" />
            </Grid>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Lowercase_x_Uid_on_Avalonia_is_not_flagged()
    {
        // Avalonia doesn't use .resw; the rule's dialect mask filters it out.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button x:Uid="loginButton" />
            </Grid>
            """,
            Dialect.Avalonia);
    }

    [Fact]
    public void Uppercase_x_Uid_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
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
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
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
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
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
        // violations the same way it catches lowercase letters. Mirrors LX0300's digit case.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
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
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="one"|] />
                <Button [|x:Uid="two"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Resw_namespace_scope_path_with_uppercase_key_is_not_flagged()
    {
        // /ResourceFile/Key is the documented UWP/WinUI resw namespace-scope form:
        // the value's leading '/' routes the lookup to a named .resw; the casing
        // convention applies to the key segment after the last '/'.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <TextBlock x:Uid="/resources/Description" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Resw_namespace_scope_path_with_lowercase_key_is_flagged()
    {
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <TextBlock [|x:Uid="/resources/description"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Resw_namespace_scope_path_with_mixed_case_filename_and_lowercase_key_is_flagged()
    {
        // Only the segment after the final '/' is the resource key; a PascalCase
        // .resw filename does not rescue a lowercase key.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Button [|x:Uid="/Resources/loginButton"|] />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Trailing_slash_resw_namespace_scope_path_is_not_flagged()
    {
        // No key segment to evaluate — don't fire.
        XamlDiagnosticVerifier<LX0301_XUidCasing>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <TextBlock x:Uid="/resources/" />
            </Grid>
            """,
            Dialect.WinUI3);
    }
}
