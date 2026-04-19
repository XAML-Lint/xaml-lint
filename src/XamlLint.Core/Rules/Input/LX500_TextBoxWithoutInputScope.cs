using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX500",
    UpstreamId = "RXT150",
    Title = "TextBox lacks InputScope",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX500.md")]
public sealed partial class LX500_TextBoxWithoutInputScope : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "TextBox") continue;
            if (element.Attribute("InputScope") is not null) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "TextBox should set InputScope to hint the on-screen keyboard and IME behavior.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
