using System.Xml;
using System.Xml.Linq;

namespace XamlLint.Core;

/// <summary>
/// A parsed XAML file plus the raw source it came from. Production code loads via
/// <see cref="FromFileAsync"/>; tests use <see cref="FromString"/>. Never throws from loading —
/// parse errors land in <see cref="ParseError"/> and callers emit LX001.
/// </summary>
public sealed class XamlDocument
{
    public string FilePath { get; }
    public string Source { get; }
    public Dialect Dialect { get; }
    public XDocument? Document { get; }
    public XElement? Root => Document?.Root;
    public XamlParseError? ParseError { get; }

    private XamlDocument(string filePath, string source, Dialect dialect, XDocument? document, XamlParseError? error)
    {
        FilePath = filePath;
        Source = source;
        Dialect = dialect;
        Document = document;
        ParseError = error;
    }

    public static XamlDocument FromString(string source, string filePath, Dialect dialect)
    {
        try
        {
            var doc = XDocument.Parse(source, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
            return new XamlDocument(filePath, source, dialect, doc, error: null);
        }
        catch (XmlException ex)
        {
            var err = new XamlParseError(ex.Message, ex.LineNumber, ex.LinePosition);
            return new XamlDocument(filePath, source, dialect, document: null, err);
        }
    }

    public static async Task<XamlDocument> FromFileAsync(string filePath, Dialect dialect, CancellationToken ct = default)
    {
        var source = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
        return FromString(source, filePath, dialect);
    }
}

public sealed record XamlParseError(string Message, int Line, int Column);
