using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Deprecated;

[XamlRule(
    Id = "LX601",
    UpstreamId = "RXT320",
    Title = "Line.Fill has no effect",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX601.md")]
public sealed partial class LX601_LineFillNoEffect : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Line") continue;

            var fill = element.Attribute("Fill");
            if (fill is null) continue;

            var span = LocationHelpers.GetAttributeSpan(fill, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Line has no interior surface; Fill has no visible effect. Use Stroke to color the line.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
