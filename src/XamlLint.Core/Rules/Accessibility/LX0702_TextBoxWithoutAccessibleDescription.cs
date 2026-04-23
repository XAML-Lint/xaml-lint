using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX0702",
    UpstreamId = "RXT601",
    Title = "TextBox lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.Wpf | Dialect.WinUI3 | Dialect.Uwp | Dialect.Avalonia | Dialect.Uno,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0702.md")]
public sealed partial class LX0702_TextBoxWithoutAccessibleDescription : IXamlRule
{
    // Name/x:Name/Header/AutomationProperties.Name suppress on any non-empty value.
    // AutomationProperties.LabeledBy is value-dependent — see LabeledByEscapeHelper.
    private static readonly string[] SimplePresenceEscapeAttributes =
    {
        "Header",
        "AutomationProperties.Name",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "TextBox") continue;
            if (HasNameEscape(element)) continue;
            if (HasSimplePresenceEscape(element)) continue;
            if (LabeledByEscapeHelper.Suppresses(element, context)) continue;
            if (LabelTargetEscapeHelper.Suppresses(element, context)) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "TextBox has no accessibility description; screen readers cannot announce its purpose. Set AutomationProperties.Name, Header, or AutomationProperties.LabeledBy=\"{x:Reference <label>}\".",
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

    private static bool HasSimplePresenceEscape(XElement element)
    {
        foreach (var name in SimplePresenceEscapeAttributes)
        {
            var attr = element.Attribute(name);
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value)) return true;
        }
        return false;
    }
}
