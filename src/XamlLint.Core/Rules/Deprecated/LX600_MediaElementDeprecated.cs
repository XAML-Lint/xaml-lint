using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Deprecated;

[XamlRule(
    Id = "LX600",
    UpstreamId = "RXT402",
    Title = "MediaElement is deprecated — use MediaPlayerElement",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX600.md")]
public sealed partial class LX600_MediaElementDeprecated : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "MediaElement") continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "MediaElement is deprecated; use MediaPlayerElement instead.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
