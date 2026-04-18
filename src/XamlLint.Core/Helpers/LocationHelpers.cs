using System.Xml;
using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Converts XDocument line info (which points at the start of element/attribute names) into
/// full <c>name="value"</c> spans against the raw source text. Required because
/// <see cref="IXmlLineInfo"/> does not expose end positions.
/// </summary>
public static class LocationHelpers
{
    /// <summary>
    /// Returns the 1-based (StartLine, StartCol, EndLine, EndCol) range for an attribute,
    /// covering the attribute name, equals sign, opening quote, value, and closing quote.
    /// End position is one past the closing quote (exclusive) — matches the convention used
    /// by the marker harness.
    /// </summary>
    public static (int StartLine, int StartCol, int EndLine, int EndCol) GetAttributeSpan(
        XAttribute attribute,
        ReadOnlyMemory<char> source)
    {
        var lineInfo = (IXmlLineInfo)attribute;
        if (!lineInfo.HasLineInfo())
            throw new InvalidOperationException(
                "Attribute has no line info; XamlDocument must load with LoadOptions.SetLineInfo.");

        var startLine = lineInfo.LineNumber;
        var startCol = lineInfo.LinePosition;

        var startOffset = LineColToOffset(source.Span, startLine, startCol);

        // Scan forward for the first '=' that is not inside whitespace.
        var eqOffset = IndexOf(source.Span, startOffset, '=');
        if (eqOffset < 0)
            throw new InvalidOperationException(
                $"Could not find '=' after attribute at line {startLine}, col {startCol}.");

        // Skip whitespace after '='.
        var quoteOffset = eqOffset + 1;
        while (quoteOffset < source.Length && char.IsWhiteSpace(source.Span[quoteOffset]))
            quoteOffset++;

        if (quoteOffset >= source.Length)
            throw new InvalidOperationException(
                $"Attribute at line {startLine}, col {startCol} has no value.");

        var quoteChar = source.Span[quoteOffset];
        if (quoteChar != '"' && quoteChar != '\'')
            throw new InvalidOperationException(
                $"Attribute value at line {startLine}, col {startCol} not quoted; found '{quoteChar}'.");

        var closeQuoteOffset = IndexOf(source.Span, quoteOffset + 1, quoteChar);
        if (closeQuoteOffset < 0)
            throw new InvalidOperationException(
                $"Unterminated attribute value starting at line {startLine}, col {startCol}.");

        var endOffset = closeQuoteOffset + 1;  // exclusive: one past the close quote
        var (endLine, endCol) = OffsetToLineCol(source.Span, endOffset);
        return (startLine, startCol, endLine, endCol);
    }

    private static int LineColToOffset(ReadOnlySpan<char> source, int line, int col)
    {
        // 1-based line and col. Scan forward line-by-line, then add col - 1.
        int offset = 0, currentLine = 1;
        while (currentLine < line && offset < source.Length)
        {
            if (source[offset] == '\n') currentLine++;
            offset++;
        }
        return offset + (col - 1);
    }

    private static (int Line, int Col) OffsetToLineCol(ReadOnlySpan<char> source, int offset)
    {
        int line = 1, col = 1;
        for (var i = 0; i < offset && i < source.Length; i++)
        {
            if (source[i] == '\n') { line++; col = 1; }
            else { col++; }
        }
        return (line, col);
    }

    /// <summary>
    /// Returns the 1-based span that covers an element's opening-tag name only (for example,
    /// <c>Grid.Row</c> inside <c>&lt;Grid.Row&gt;</c>, or <c>x:Key</c> inside
    /// <c>&lt;x:Key&gt;</c>). Used by rules whose diagnostic source is an <see cref="XElement"/>
    /// — typically element-syntax attached properties. Line info on <see cref="XElement"/>
    /// points at the first character of the source name, including any XML prefix; the end
    /// column advances by the prefix length (plus a colon) when the element is prefixed.
    /// The returned <c>EndCol</c> is one past the last character of the name (exclusive),
    /// consistent with <see cref="GetAttributeSpan"/>.
    /// </summary>
    public static (int StartLine, int StartCol, int EndLine, int EndCol) GetElementNameSpan(
        XElement element)
    {
        var lineInfo = (IXmlLineInfo)element;
        if (!lineInfo.HasLineInfo())
            throw new InvalidOperationException(
                "Element has no line info; XamlDocument must load with LoadOptions.SetLineInfo.");

        var line = lineInfo.LineNumber;
        var startCol = lineInfo.LinePosition;

        // LinePosition points at the first character of the source name, including any XML
        // prefix. LocalName strips the prefix, so for <x:Key> (source length 5) LocalName is
        // "Key" (length 3). Add prefix length + ':' when present to keep EndCol at the true
        // end of the name.
        var prefix = element.GetPrefixOfNamespace(element.Name.Namespace);
        var nameLen = element.Name.LocalName.Length
            + (string.IsNullOrEmpty(prefix) ? 0 : prefix.Length + 1);

        return (line, startCol, line, startCol + nameLen);
    }

    private static int IndexOf(ReadOnlySpan<char> source, int startOffset, char target)
    {
        for (var i = startOffset; i < source.Length; i++)
            if (source[i] == target) return i;
        return -1;
    }
}
