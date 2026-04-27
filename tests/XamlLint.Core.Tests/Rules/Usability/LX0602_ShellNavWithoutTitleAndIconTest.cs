using XamlLint.Core.Rules.Usability;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Usability;

public sealed class LX0602_ShellNavWithoutTitleAndIconTest
{
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void ShellContent_without_Title_and_Icon_is_flagged()
    {
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <TabBar>
                    <[|ShellContent|] />
                </TabBar>
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ShellContent_with_Title_only_is_not_flagged()
    {
        // Either Title OR Icon is enough. Text-only nav patterns are valid.
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <TabBar>
                    <ShellContent Title="Home" />
                </TabBar>
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ShellContent_with_Icon_only_is_not_flagged()
    {
        // Icon-only nav patterns are valid (compact tabs on small screens).
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <TabBar>
                    <ShellContent Icon="home.png" />
                </TabBar>
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ShellContent_with_both_Title_and_Icon_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <TabBar>
                    <ShellContent Title="Home" Icon="home.png" />
                </TabBar>
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void ShellContent_with_empty_Title_and_empty_Icon_is_flagged()
    {
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <TabBar>
                    <[|ShellContent|] Title="" Icon="" />
                </TabBar>
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Tab_without_Title_and_Icon_is_flagged()
    {
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <TabBar>
                    <[|Tab|] />
                </TabBar>
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void FlyoutItem_without_Title_and_Icon_is_flagged()
    {
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <[|FlyoutItem|] />
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void MenuItem_without_Title_and_Icon_is_flagged()
    {
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <Shell xmlns="{{Maui}}">
                <[|MenuItem|] />
            </Shell>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Non_Shell_element_with_no_Title_and_Icon_is_not_flagged()
    {
        // The rule is scoped to MAUI Shell nav-surface element local names; other elements
        // never fire it.
        XamlDiagnosticVerifier<LX0602_ShellNavWithoutTitleAndIcon>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}">
                <Label />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
