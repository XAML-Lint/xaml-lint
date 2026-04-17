using System.Xml;
using System.Xml.Linq;

namespace XamlLint.Core.Tests;

public sealed class XamlDocumentTest
{
    [Fact]
    public void Load_from_string_preserves_source_text()
    {
        const string src = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />";
        var doc = XamlDocument.FromString(src, "inline.xaml", Dialect.Wpf);

        doc.Source.Should().Be(src);
        doc.FilePath.Should().Be("inline.xaml");
        doc.Dialect.Should().Be(Dialect.Wpf);
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("Grid");
    }

    [Fact]
    public void Load_preserves_line_info_on_elements()
    {
        const string src = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\n    <Button />\n</Grid>";
        var doc = XamlDocument.FromString(src, "inline.xaml", Dialect.Wpf);

        var button = doc.Root!.Element(XName.Get("Button", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"))!;
        var info = (IXmlLineInfo)button;
        info.HasLineInfo().Should().BeTrue();
        info.LineNumber.Should().Be(2);
    }

    [Fact]
    public void Load_captures_parse_error_without_throwing()
    {
        const string malformed = "<Grid><Button></Grid>";
        var doc = XamlDocument.FromString(malformed, "bad.xaml", Dialect.Wpf);

        doc.ParseError.Should().NotBeNull();
        doc.Root.Should().BeNull();
    }
}
