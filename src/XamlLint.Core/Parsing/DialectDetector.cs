using System.Xml;
using System.Xml.Linq;

namespace XamlLint.Core.Parsing;

/// <summary>
/// Implements the definitive xmlns sniff step of the dialect cascade (spec §4, step 1). Only
/// dialects with a distinctive root namespace URL are sniffable — WPF and UWP/WinUI 3 share
/// <c>winfx/2006/xaml/presentation</c>, so those cases return <c>null</c> and the caller
/// falls through to --dialect, config, and finally the fallback.
/// </summary>
public static class DialectDetector
{
    public const Dialect Fallback = Dialect.Wpf;

    private const string MauiUri     = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string AvaloniaUri = "https://github.com/avaloniaui";

    /// <summary>
    /// Peeks at two signals on the root element: its own effective namespace (resolved via
    /// the prefix, if any) and its declared default xmlns. Either one being a definitive
    /// dialect URI settles the document. Returns <c>null</c> when parsing fails, when no
    /// namespaces are declared, or when the signals only reveal the shared WPF/WinUI/UWP
    /// presentation URL.
    /// </summary>
    /// <remarks>
    /// Checking the default xmlns in addition to the element's own namespace catches common
    /// MAUI idioms where the root is a custom type from a <c>using:…</c> CLR namespace but
    /// the document's default xmlns declares MAUI (e.g., Uno's <c>MauiEmbedding</c> Syncfusion
    /// samples: <c>&lt;localCore:SampleView xmlns="…maui" xmlns:localCore="using:…"&gt;</c>).
    /// </remarks>
    public static Dialect? Sniff(string source)
    {
        try
        {
            var doc = XDocument.Parse(source);
            var root = doc.Root;
            if (root is null) return null;

            var rootNs = root.Name.NamespaceName;
            var defaultNs = root.GetDefaultNamespace().NamespaceName;

            return Match(rootNs) ?? Match(defaultNs);
        }
        catch (XmlException)
        {
            return null;
        }

        static Dialect? Match(string? ns) => ns switch
        {
            MauiUri     => Dialect.Maui,
            AvaloniaUri => Dialect.Avalonia,
            _ => null,
        };
    }
}
