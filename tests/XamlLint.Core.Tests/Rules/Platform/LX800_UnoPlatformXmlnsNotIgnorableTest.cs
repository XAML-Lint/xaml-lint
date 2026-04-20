using XamlLint.Core.Rules.Platform;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Platform;

public sealed class LX800_UnoPlatformXmlnsNotIgnorableTest
{
    private const string WinUI = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [Fact]
    public void Uno_android_xmlns_without_mc_Ignorable_is_flagged()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}"
                  [|xmlns:android="http://uno.ui/android"|]>
            </Page>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Uno_android_xmlns_listed_in_mc_Ignorable_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}"
                  xmlns:android="http://uno.ui/android"
                  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                  mc:Ignorable="android">
            </Page>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Multiple_Uno_xmlns_with_only_some_ignorable_flags_the_missing_ones()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}"
                  xmlns:android="http://uno.ui/android"
                  [|xmlns:ios="http://uno.ui/ios"|]
                  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                  mc:Ignorable="android">
            </Page>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Multiple_Uno_xmlns_all_listed_in_mc_Ignorable_are_not_flagged()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}"
                  xmlns:android="http://uno.ui/android"
                  xmlns:ios="http://uno.ui/ios"
                  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                  mc:Ignorable="android ios">
            </Page>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Ignorable_declared_under_non_standard_prefix_is_respected()
    {
        // The rule looks up Ignorable by expanded XName, so whatever prefix the author bound to
        // the markup-compatibility namespace must work — not just 'mc'. Guard against a future
        // regression that hardcodes the 'mc' prefix.
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}"
                  xmlns:android="http://uno.ui/android"
                  xmlns:compat="http://schemas.openxmlformats.org/markup-compatibility/2006"
                  compat:Ignorable="android">
            </Page>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void Document_without_any_Uno_xmlns_does_not_fire()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}">
                <TextBlock Text="hello" />
            </Page>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_uno_platform_specific_xmlns_does_not_fire()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <Page xmlns="{WinUI}"
                  xmlns:local="clr-namespace:MyApp">
            </Page>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX800_UnoPlatformXmlnsNotIgnorable>.Analyze(
            $"""
            <!-- xaml-lint disable LX800 -->
            <Page xmlns="{WinUI}"
                  xmlns:android="http://uno.ui/android">
            </Page>
            """,
            Dialect.Uno);
    }
}
