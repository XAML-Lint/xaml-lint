namespace XamlLint.Core.Helpers;

public sealed record MarkupExtensionInfo(string Name, IReadOnlyDictionary<string, string> NamedArguments);

/// <summary>
/// Lightweight parser for XAML markup extensions (<c>{Name args}</c>). Not a full grammar —
/// enough to answer "is this a markup extension?" and "what are the top-level named
/// arguments?". Handles nested <c>{…}</c> inside argument values (e.g.,
/// <c>Converter={StaticResource Bool}</c>) by tracking brace depth during the comma split.
/// </summary>
public static class MarkupExtensionHelpers
{
    public static bool IsMarkupExtension(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var trimmed = value.AsSpan().Trim();
        if (trimmed.Length < 3) return false;                         // need "{X}" at minimum
        if (trimmed[0] != '{' || trimmed[^1] != '}') return false;
        if (trimmed.Length >= 2 && trimmed[1] == '{') return false;   // {{ is escape — not an extension
        return true;
    }

    public static bool TryParseExtension(string? value, out MarkupExtensionInfo info)
    {
        info = null!;
        if (!IsMarkupExtension(value)) return false;

        var inner = value!.AsSpan().Trim();
        inner = inner[1..^1].Trim();  // strip surrounding braces
        if (inner.Length == 0) return false;

        // Extension name is the first whitespace- or comma-delimited token (may contain ':').
        var nameEnd = 0;
        while (nameEnd < inner.Length && !char.IsWhiteSpace(inner[nameEnd]) && inner[nameEnd] != ',')
            nameEnd++;
        var name = inner[..nameEnd].ToString();
        if (name.Length == 0) return false;

        var rest = nameEnd < inner.Length ? inner[nameEnd..].Trim() : ReadOnlySpan<char>.Empty;

        var named = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var arg in SplitTopLevelCommas(rest))
        {
            var eq = IndexOfTopLevelEquals(arg);
            if (eq < 0) continue;  // positional, skip — M2 doesn't need them
            var key = arg[..eq].Trim().ToString();
            var val = StripSurroundingQuotes(arg[(eq + 1)..].Trim().ToString());
            if (key.Length > 0) named[key] = val;
        }

        info = new MarkupExtensionInfo(name, named);
        return true;
    }

    private static IEnumerable<string> SplitTopLevelCommas(ReadOnlySpan<char> source)
    {
        // Can't yield inside a ref-struct method, so materialise to list first.
        var results = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < source.Length; i++)
        {
            switch (source[i])
            {
                case '{': depth++; break;
                case '}': depth--; break;
                case ',' when depth == 0:
                    results.Add(source[start..i].ToString());
                    start = i + 1;
                    break;
            }
        }
        if (start < source.Length)
            results.Add(source[start..].ToString());
        return results;
    }

    /// <summary>
    /// Strips a single surrounding pair of <c>'…'</c> or <c>"…"</c>. XAML markup-extension
    /// argument values may be quoted to contain delimiter characters (comma, equals,
    /// whitespace) without terminating the token — the quotes are syntactic, not part of
    /// the value. No effect on nested markup extensions (values starting with <c>{</c>).
    /// </summary>
    private static string StripSurroundingQuotes(string value)
    {
        if (value.Length < 2) return value;
        var first = value[0];
        if (first != '\'' && first != '"') return value;
        if (value[^1] != first) return value;
        return value.Substring(1, value.Length - 2);
    }

    private static int IndexOfTopLevelEquals(string arg)
    {
        var depth = 0;
        for (var i = 0; i < arg.Length; i++)
        {
            switch (arg[i])
            {
                case '{': depth++; break;
                case '}': depth--; break;
                case '=' when depth == 0: return i;
            }
        }
        return -1;
    }
}
