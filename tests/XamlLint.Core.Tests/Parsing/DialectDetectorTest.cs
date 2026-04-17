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
