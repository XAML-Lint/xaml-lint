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
    public void MediaElement_on_Uno_is_flagged()
    {
        // Uno provides MediaPlayerElement as the preferred replacement on its WinUI-compatible
        // stack; MediaElement inherits the UWP/WinUI deprecation.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <[|MediaElement|] Source="video.mp4" />
            </Grid>
            """,
            Dialect.Uno);
    }

    [Fact]
    public void MediaElement_on_Wpf_is_not_flagged()
    {
        // WPF's MediaElement is current; MediaPlayerElement is UWP/WinUI/Uno only.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void MediaElement_on_Maui_is_not_flagged()
    {
        // MAUI's MediaElement (CommunityToolkit) is independent of the UWP/WinUI deprecation.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void MediaElement_on_Avalonia_is_not_flagged()
    {
        // Avalonia does not ship MediaPlayerElement; the deprecation does not apply there.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <MediaElement Source="video.mp4" />
            </Grid>
            """,
            Dialect.Avalonia);
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
    public void MediaElement_inside_DataTemplate_is_flagged()
    {
        // `DescendantsAndSelf()` walks into templates; template-internal MediaElement is still
        // a deprecation violation.
        XamlDiagnosticVerifier<LX600_MediaElementDeprecated>.Analyze(
            """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <ListView>
                    <ListView.ItemTemplate>
                        <DataTemplate>
                            <[|MediaElement|] Source="{Binding Source}" />
                        </DataTemplate>
                    </ListView.ItemTemplate>
                </ListView>
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
