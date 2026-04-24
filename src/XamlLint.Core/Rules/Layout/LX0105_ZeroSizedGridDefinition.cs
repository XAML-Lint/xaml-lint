using System.Globalization;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Layout;

[XamlRule(
    Id = "LX0105",
    UpstreamId = null,
    Title = "Zero-sized RowDefinition / ColumnDefinition",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0105.md")]
public sealed partial class LX0105_ZeroSizedGridDefinition : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            string attrName;
            string definitionKind;
            if (element.Name.LocalName == GridAncestryHelpers.RowDefinitionElement)
            {
                attrName = "Height";
                definitionKind = "RowDefinition";
            }
            else if (element.Name.LocalName == GridAncestryHelpers.ColumnDefinitionElement)
            {
                attrName = "Width";
                definitionKind = "ColumnDefinition";
            }
            else
            {
                continue;
            }

            var attr = element.Attribute(attrName);
            if (attr is null) continue;

            var kind = TryClassifyZeroSized(attr.Value);
            if (kind is null) continue;

            var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
            var reason = kind switch
            {
                ZeroSizedKind.Zero => "is zero",
                ZeroSizedKind.Negative => $"is negative ({attr.Value.Trim()})",
                ZeroSizedKind.Empty => "is empty",
                _ => throw new InvalidOperationException($"Unexpected kind {kind}.")
            };

            var axis = definitionKind.Replace("Definition", "").ToLowerInvariant();

            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"{definitionKind} {attrName} {reason}; the {axis} will render at zero pixels and disappear at runtime.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private enum ZeroSizedKind { Zero, Negative, Empty }

    private static ZeroSizedKind? TryClassifyZeroSized(string raw)
    {
        var t = raw.Trim();
        if (t.Length == 0) return ZeroSizedKind.Empty;

        // Star-sized ("*", "2*", "0*", "0.5*") — not a literal pixel value. Skip.
        if (t.EndsWith('*')) return null;

        // Auto-sized — GridLength parsing is case-insensitive on every dialect.
        if (string.Equals(t, "Auto", StringComparison.OrdinalIgnoreCase)) return null;

        // Literal pixel value. Invariant culture so '.' is always the decimal mark.
        if (!double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return null;

        if (value < 0) return ZeroSizedKind.Negative;
        if (value == 0) return ZeroSizedKind.Zero;
        return null;
    }
}
