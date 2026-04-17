using System.Xml;
using System.Xml.Linq;

namespace XamlLint.Core.Parsing;

/// <summary>
/// Implements the xmlns sniff step of the dialect cascade (spec §4, step 4). Only definitive
/// dialects are sniffable — WPF and UWP/WinUI 3 share the <c>winfx/2006/xaml/presentation</c>
/// URL, so those cases return <c>null</c> and the caller falls through to the next step.
/// </summary>
public static class DialectDetector
{
    public const Dialect Fallback = Dialect.Wpf;

    private const string MauiUri     = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string AvaloniaUri = "https://github.com/avaloniaui";

    /// <summary>
    /// Peeks at the root element's default xmlns. Returns <c>null</c> when parsing fails or
    /// the URL isn't one of the definitive dialect-identifying URLs.
    /// </summary>
    public static Dialect? Sniff(string source)
    {
        try
        {
            var doc = XDocument.Parse(source);
            var rootNs = doc.Root?.Name.NamespaceName;
            if (string.IsNullOrEmpty(rootNs)) return null;

            return rootNs switch
            {
                MauiUri     => Dialect.Maui,
                AvaloniaUri => Dialect.Avalonia,
                _ => null,
            };
        }
        catch (XmlException)
        {
            return null;
        }
    }
}
