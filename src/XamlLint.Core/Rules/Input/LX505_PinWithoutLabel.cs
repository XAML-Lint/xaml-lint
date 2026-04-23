using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX505",
    UpstreamId = "RXT325",
    Title = "Pin lacks Label",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX505.md")]
public sealed partial class LX505_PinWithoutLabel : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Pin") continue;
            if (PropertyElementHelpers.HasAttributeOrPropertyElement(element, "Label")) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Pin requires a Label; MAUI Maps throws ArgumentException at runtime without one.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
