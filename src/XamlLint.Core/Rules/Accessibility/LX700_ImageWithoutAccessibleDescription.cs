using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX700",
    UpstreamId = "RXT350",
    Title = "Image lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX700.md")]
public sealed partial class LX700_ImageWithoutAccessibleDescription : IXamlRule
{
    // Name / HelpText suppress the rule regardless of value — any value the author supplied
    // is something an AT can read out (even a bound path that resolves at runtime is a
    // deliberate statement of "I've thought about this"). LabeledBy is value-dependent now:
    // a dangling {x:Reference} no longer suppresses. IsInAccessibleTree is boolean and only
    // False (or a bound value) means "decorative — skip in AT."
    //
    // AutomationId is the test-automation hook on UI Automation (Windows) and MAUI's
    // Microsoft.Maui.IElement.AutomationId; upstream Rapid XAML Toolkit RXT350/RXT351 treat
    // its presence as an "author thought about this" signal, so we match that.
    private static readonly string[] PresenceEscapeAttributes =
    {
        "AutomationProperties.Name",
        "AutomationProperties.HelpText",
        "SemanticProperties.Description",
        "SemanticProperties.Hint",
        "AutomationId",
    };

    private const string IsInAccessibleTreeAttribute = "AutomationProperties.IsInAccessibleTree";

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Image") continue;
            if (HasAnyEscape(element, context)) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Image has no accessibility description; screen readers cannot convey its meaning. Set AutomationProperties.Name or AutomationProperties.IsInAccessibleTree=\"False\" to mark it decorative.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static bool HasAnyEscape(XElement element, RuleContext context)
    {
        foreach (var name in PresenceEscapeAttributes)
        {
            if (element.Attribute(name) is not null) return true;
        }

        if (LabeledByEscapeHelper.Suppresses(element, context)) return true;

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
