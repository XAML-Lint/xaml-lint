using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX700",
    UpstreamId = "RXT350",
    Title = "Image lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX700.md")]
public sealed partial class LX700_ImageWithoutAccessibleDescription : IXamlRule
{
    private static readonly string[] EscapeAttributes =
    {
        "AutomationProperties.Name",
        "AutomationProperties.HelpText",
        "AutomationProperties.LabeledBy",
        "AutomationProperties.IsInAccessibleTree",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Image") continue;
            if (HasAnyEscape(element)) continue;

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

    private static bool HasAnyEscape(System.Xml.Linq.XElement element)
    {
        foreach (var name in EscapeAttributes)
        {
            if (element.Attribute(name) is not null) return true;
        }
        return false;
    }
}
