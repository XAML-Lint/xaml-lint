using XamlLint.Core.Rules.Deprecated;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Deprecated;

public sealed class LX600_MediaElementDeprecatedTest
{
    [Fact]
    public void MediaElement_on_WinUI3_is_flagged()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="video.mp4" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void MediaElement_on_Uwp_is_flagged()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="video.mp4" />
            </Grid>
            """,
            Dialect.Uwp);
    }

    [Fact]
    public void MediaElement_on_Wpf_is_not_flagged()
    {
        // WPF's MediaElement is current; MediaPlayerElement is UWP/WinUI only.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void MediaPlayerElement_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaPlayerElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Multiple_MediaElements_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="a.mp4" />
                <[|MediaElement|] Source="b.mp4" />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }
}
