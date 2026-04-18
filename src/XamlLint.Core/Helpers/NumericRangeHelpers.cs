using System.Globalization;
using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Small utilities for the <c>Minimum</c>/<c>Maximum</c> range rules (LX501, LX502).
/// Only literal, invariant-culture-parseable values compare; anything else (a markup
/// extension, an empty string, a non-numeric literal) returns <c>null</c> so the rule
/// skips the pair rather than producing a false positive.
/// </summary>
public static class NumericRangeHelpers
{
    /// <summary>
    /// Parses an attribute's value as an invariant-culture <see cref="double"/>. Returns
    /// <c>null</c> when <paramref name="attribute"/> is <c>null</c>, empty/whitespace, a
    /// markup extension (<c>{Binding …}</c>, <c>{StaticResource …}</c>, …), or an unparseable
    /// literal.
    /// </summary>
    public static double? TryReadLiteralDouble(XAttribute? attribute)
    {
        if (attribute is null) return null;
        var value = attribute.Value;
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (MarkupExtensionHelpers.IsMarkupExtension(value)) return null;

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
