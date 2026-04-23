using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX0703",
    Title = "Entry lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0703.md")]
public sealed partial class LX0703_EntryWithoutAccessibleDescription : IXamlRule
{
    // Each suppressor requires a non-empty value to fire. An empty string
    // (e.g. AutomationProperties.Name="") is treated as "still missing" rather
    // than suppressing — an empty a11y string is an obvious authoring mistake
    // that a screen reader announces as silence. (LX0700/LX0701 use presence-only
    // checks for historical reasons; harmonisation across LX70x is deferred.)
    private static readonly string[] PresenceEscapeAttributes =
    {
        "SemanticProperties.Description",
        "SemanticProperties.Hint",
        "AutomationProperties.Name",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Entry") continue;
            if (HasNameEscape(element)) continue;
            if (HasPresenceEscape(element)) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Entry has no accessibility description; screen readers cannot announce its purpose. Set SemanticProperties.Description or SemanticProperties.Hint.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static bool HasNameEscape(XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            var isXName = attr.Name.LocalName == "Name" && XamlNamespaces.IsXamlNamespace(attr.Name.NamespaceName);
            if (isXName && !string.IsNullOrWhiteSpace(attr.Value)) return true;
        }
        var unprefixed = element.Attribute("Name");
        return unprefixed is not null && !string.IsNullOrWhiteSpace(unprefixed.Value);
    }

    private static bool HasPresenceEscape(XElement element)
    {
        foreach (var name in PresenceEscapeAttributes)
        {
            var attr = element.Attribute(name);
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value)) return true;
        }
        return false;
    }
}
