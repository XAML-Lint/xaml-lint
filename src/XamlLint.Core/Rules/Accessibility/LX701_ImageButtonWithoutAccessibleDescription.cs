using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX701",
    UpstreamId = "RXT351",
    Title = "ImageButton lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX701.md")]
public sealed partial class LX701_ImageButtonWithoutAccessibleDescription : IXamlRule
{
    // Name / HelpText / LabeledBy suppress on any value. IsInAccessibleTree is the
    // decorative opt-out and must be literal False (or bound) — "True" reasserts default
    // inclusion and must not suppress.
    private static readonly string[] PresenceEscapeAttributes =
    {
        "AutomationProperties.Name",
        "AutomationProperties.HelpText",
        "AutomationProperties.LabeledBy",
    };

    private const string IsInAccessibleTreeAttribute = "AutomationProperties.IsInAccessibleTree";

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "ImageButton") continue;
            if (HasAnyEscape(element)) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "ImageButton has no accessibility description; screen readers cannot convey its function. Set AutomationProperties.Name or AutomationProperties.IsInAccessibleTree=\"False\" to mark it decorative.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static bool HasAnyEscape(XElement element)
    {
        foreach (var name in PresenceEscapeAttributes)
        {
            if (element.Attribute(name) is not null) return true;
        }

        var treeAttr = element.Attribute(IsInAccessibleTreeAttribute);
        if (treeAttr is not null)
        {
            var value = treeAttr.Value;
            if (MarkupExtensionHelpers.IsMarkupExtension(value)) return true;
            if (string.Equals(value.Trim(), "False", StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}
