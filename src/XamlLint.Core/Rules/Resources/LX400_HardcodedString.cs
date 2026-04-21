using System.Text;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Resources;

[XamlRule(
    Id = "LX400",
    UpstreamId = "RXT200",
    Title = "Hardcoded string; use a resource",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX400.md")]
public sealed partial class LX400_HardcodedString : IXamlRule
{
    // Conservative baseline list of text-presenting attributes that commonly need localisation.
    // Expand as real-world false-negatives surface.
    private static readonly HashSet<string> TextAttributeNames = new(StringComparer.Ordinal)
    {
        "Text",
        "Title",
        "Header",
        "ToolTip",
        "Content",
        "PlaceholderText",
        "Placeholder",
        "Description",
        "Watermark",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (!TextAttributeNames.Contains(attr.Name.LocalName)) continue;
                // Only inspect unprefixed attributes. Anything with a prefix (x:Uid, Grid.Row,
                // d:DesignHeight, custom dialect attributes, etc.) is out of scope for hardcoded-string
                // detection — those names aren't in our TextAttributeNames list anyway, and the prefix
                // guarantees it's not a user-visible text property of a standard control.
                if (attr.Name.NamespaceName.Length != 0) continue;

                var value = attr.Value;
                if (string.IsNullOrWhiteSpace(value)) continue;
                if (MarkupExtensionHelpers.IsMarkupExtension(value)) continue;
                if (IsSymbolOrGlyph(value)) continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"Hardcoded string on '{attr.Name.LocalName}' should be moved to a resource.",
                    File: document.FilePath,
                    StartLine: span.StartLine,
                    StartCol: span.StartCol,
                    EndLine: span.EndLine,
                    EndCol: span.EndCol,
                    HelpUri: Metadata.HelpUri);
            }
        }
    }

    // Values whose non-whitespace code points contain no letters and no digits aren't
    // localisable prose. Covers two overlapping conventions:
    //   - icon-font glyphs in the Unicode Private Use Area (Segoe MDL2 Assets, Segoe
    //     Fluent Icons, Material Icons, FontAwesome, and similar) -- PUA code points
    //     are not classified as letters or digits by Unicode category;
    //   - UI-chrome punctuation like "+", "-", ":", "/" used as button captions or
    //     separators.
    // Letters and digits still fire: a single letter like "X" could be localisable copy,
    // and a digit like "1" becomes different glyphs in Arabic/Thai/etc. locales. Mixed
    // values such as "+ Add" fire because of the prose segment.
    private static bool IsSymbolOrGlyph(string value)
    {
        var sawNonWhitespace = false;
        foreach (var rune in value.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune)) continue;
            sawNonWhitespace = true;
            if (Rune.IsLetter(rune) || Rune.IsDigit(rune)) return false;
        }
        return sawNonWhitespace;
    }
}
