using XamlLint.Core.Parsing;

namespace XamlLint.Core.Tests.Parsing;

public sealed class DialectDetectorTest
{
    [Theory]
    [InlineData("http://schemas.microsoft.com/dotnet/2021/maui", Dialect.Maui)]
    [InlineData("https://github.com/avaloniaui", Dialect.Avalonia)]
    public void Detects_definitive_root_namespace(string rootXmlns, Dialect expected)
    {
        var xaml = $"<Root xmlns=\"{rootXmlns}\" />";
        DialectDetector.Sniff(xaml).Should().Be(expected);
    }

    [Fact]
    public void Returns_null_when_root_xmlns_is_ambiguous_wpf_presentation_url()
    {
        // WPF and UWP/WinUI 3 share this URL; the sniff can't decide. Returns null so callers fall through.
        const string xaml = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />";
        DialectDetector.Sniff(xaml).Should().BeNull();
    }

    [Fact]
    public void Detects_maui_when_declared_as_default_xmlns_on_prefixed_root()
    {
        // Real pattern from Uno.Samples/UI/MauiEmbedding Syncfusion samples: the root is a
        // custom type from a using:… CLR namespace (so the root's own Name.NamespaceName is
        // using:…, not a dialect URI), but its xmlns="" default is MAUI. The document IS a
        // MAUI document — the default-xmlns declaration on the root says so unambiguously.
        const string xaml = """
            <localCore:SampleView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                                  xmlns:localCore="using:SyncfusionApp.MauiControls.Samples.Base" />
            """;
        DialectDetector.Sniff(xaml).Should().Be(Dialect.Maui);
    }

    [Fact]
    public void Detects_avalonia_when_declared_as_default_xmlns_on_prefixed_root()
    {
        const string xaml = """
            <local:MyView xmlns="https://github.com/avaloniaui"
                          xmlns:local="using:SomeApp.Views" />
            """;
        DialectDetector.Sniff(xaml).Should().Be(Dialect.Avalonia);
    }

    [Fact]
    public void Returns_null_for_malformed_xaml()
    {
        DialectDetector.Sniff("<not-xml").Should().BeNull();
    }

    [Fact]
    public void Returns_null_when_no_root_default_namespace()
    {
        DialectDetector.Sniff("<Root />").Should().BeNull();
    }

    [Fact]
    public void Fallback_returns_wpf()
    {
        DialectDetector.Fallback.Should().Be(Dialect.Wpf);
    }
}
