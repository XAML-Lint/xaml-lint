namespace XamlLint.Core.Helpers;

/// <summary>
/// Extracts the target name from an <c>{x:Reference Foo}</c> or
/// <c>{x:Reference Name=Foo}</c> markup extension value. Returns <c>null</c> when the
/// input isn't a reference extension or the target name cannot be parsed.
/// </summary>
/// <remarks>
/// Known edge cases that currently return <c>null</c> (callers treat null as "malformed —
/// don't second-guess", which is the permissive side):
/// <list type="bullet">
///   <item><c>{x:Reference,Name=Foo}</c> — no whitespace between the extension name and
///         the first argument. Legal per XAML 2009; rarely seen in practice.</item>
///   <item><c>{x:Reference Foo, Mode=OneTime}</c> — positional + named args together.
///         The heuristic routes to the named-arg branch and drops the positional token.</item>
/// </list>
/// Both are candidates for future reconciliation with <see cref="MarkupExtensionHelpers"/>
/// (which tokenises names on whitespace OR comma and exposes only named args). Reconciling
/// requires either extending <see cref="MarkupExtensionInfo"/> to surface positional args or
/// sharing a tokenizer — out of scope here.
/// </remarks>
public static class ReferenceTargetNameHelper
{
    public static string? Extract(string value)
    {
        var trimmed = value.AsSpan().Trim();
        if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[^1] != '}') return null;
        var inner = trimmed[1..^1].Trim();

        // Skip the extension name (first token up to whitespace).
        var i = 0;
        while (i < inner.Length && !char.IsWhiteSpace(inner[i])) i++;
        if (i >= inner.Length) return null;
        var rest = inner[i..].Trim();
        if (rest.Length == 0) return null;

        // Named-argument form (e.g. Name=Foo) — delegate to MarkupExtensionHelpers.
        if (rest[0] == '{' || rest.IndexOf('=') >= 0)
        {
            if (MarkupExtensionHelpers.TryParseExtension(value, out var info)
                && info.NamedArguments.TryGetValue("Name", out var named))
                return named;
            return null;
        }

        // Positional form: first token up to whitespace or comma.
        var end = 0;
        while (end < rest.Length && !char.IsWhiteSpace(rest[end]) && rest[end] != ',') end++;
        return rest[..end].ToString();
    }
}
