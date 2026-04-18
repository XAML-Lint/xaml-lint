using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class LocationHelpersTest
{
    private static XamlDocument Doc(string xaml) =>
        XamlDocument.FromString(xaml, "f.xaml", Dialect.Wpf);

    [Fact]
    public void Unprefixed_double_quoted_attribute_span_covers_name_through_close_quote()
    {
        const string xaml = "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Text=\"hi\" />";
        var doc = Doc(xaml);
        var attr = doc.Root!.Attribute("Text")!;

        var span = LocationHelpers.GetAttributeSpan(attr, doc.Source.AsMemory());

        var expectedStart = xaml.IndexOf("Text=", StringComparison.Ordinal) + 1;
        var expectedEnd = xaml.IndexOf("\" />", StringComparison.Ordinal) + 2;
        span.StartLine.Should().Be(1);
        span.StartCol.Should().Be(expectedStart);
        span.EndLine.Should().Be(1);
        span.EndCol.Should().Be(expectedEnd);
    }

    [Fact]
    public void Prefixed_attribute_span_covers_prefix_colon_local_name_value()
    {
        const string xaml =
            "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
            "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Name=\"MyButton\" />";
        var doc = Doc(xaml);
        var attr = doc.Root!.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))!;

        var span = LocationHelpers.GetAttributeSpan(attr, doc.Source.AsMemory());

        // The substring "x:Name=\"MyButton\"" starts after the closing quote of the x: xmlns
        // value. Recompute the offsets from the literal to keep this resilient:
        var expectedStart = xaml.IndexOf("x:Name", StringComparison.Ordinal) + 1; // 1-based
        var expectedEnd = xaml.IndexOf("\" />", StringComparison.Ordinal) + 2;    // include close quote
        span.StartCol.Should().Be(expectedStart);
        span.EndCol.Should().Be(expectedEnd);
        span.StartLine.Should().Be(1);
        span.EndLine.Should().Be(1);
    }

    [Fact]
    public void Single_quoted_attribute_is_handled()
    {
        const string xaml = "<Root xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Text='hi' />";
        var doc = Doc(xaml);
        var attr = doc.Root!.Attribute("Text")!;

        var span = LocationHelpers.GetAttributeSpan(attr, doc.Source.AsMemory());

        var expectedStart = xaml.IndexOf("Text=", StringComparison.Ordinal) + 1;
        var expectedEnd = xaml.IndexOf("' />", StringComparison.Ordinal) + 2;
        span.StartCol.Should().Be(expectedStart);
        span.EndCol.Should().Be(expectedEnd);
    }

    [Fact]
    public void Multiline_attribute_value_end_line_is_correct()
    {
        const string xaml =
            "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
            "      Text=\"line-one\nline-two\" />";
        var doc = Doc(xaml);
        var attr = doc.Root!.Attribute("Text")!;

        var span = LocationHelpers.GetAttributeSpan(attr, doc.Source.AsMemory());

        span.StartLine.Should().Be(2);
        span.EndLine.Should().Be(3);
    }

    [Fact]
    public void XamlNamespaces_recognises_the_2006_and_2009_uris()
    {
        XamlNamespaces.IsXamlNamespace("http://schemas.microsoft.com/winfx/2006/xaml").Should().BeTrue();
        XamlNamespaces.IsXamlNamespace("http://schemas.microsoft.com/winfx/2009/xaml").Should().BeTrue();
        XamlNamespaces.IsXamlNamespace("http://schemas.microsoft.com/winfx/2006/xaml/presentation").Should().BeFalse();
        XamlNamespaces.IsXamlNamespace("").Should().BeFalse();
    }
}
