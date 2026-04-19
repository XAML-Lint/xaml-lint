using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Resources;

[XamlRule(
    Id = "LX400",
    UpstreamId = "RXT200",
    Title = "Hardcoded string; use a resource",
    DefaultSeverity = Severity.Info,
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
}
