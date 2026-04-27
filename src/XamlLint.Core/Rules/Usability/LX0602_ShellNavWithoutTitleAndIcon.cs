using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Usability;

[XamlRule(
    Id = "LX0602",
    Title = "MAUI Shell nav-surface lacks Title and Icon",
    DefaultSeverity = Severity.Warning,
    DefaultEnabled = true,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0602.md")]
public sealed partial class LX0602_ShellNavWithoutTitleAndIcon : IXamlRule
{
    private static readonly HashSet<string> ShellNavElementLocalNames = new(StringComparer.Ordinal)
    {
        "Tab",
        "ShellContent",
        "FlyoutItem",
        "MenuItem",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (!ShellNavElementLocalNames.Contains(element.Name.LocalName)) continue;

            var hasTitle = !string.IsNullOrWhiteSpace(element.Attribute("Title")?.Value);
            var hasIcon = !string.IsNullOrWhiteSpace(element.Attribute("Icon")?.Value);
            if (hasTitle || hasIcon) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"{element.Name.LocalName} has neither Title nor Icon; the navigation surface will render blank. Set at least one.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
