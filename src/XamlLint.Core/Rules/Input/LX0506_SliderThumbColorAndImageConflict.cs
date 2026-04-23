using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX0506",
    UpstreamId = "RXT331",
    Title = "Slider sets both ThumbColor and ThumbImageSource",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0506.md")]
public sealed partial class LX0506_SliderThumbColorAndImageConflict : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Slider") continue;

            var thumbColor = PropertyElementHelpers.TryGetValueAndSource(element, "ThumbColor");
            if (thumbColor is null) continue;
            if (!PropertyElementHelpers.HasAttributeOrPropertyElement(element, "ThumbImageSource")) continue;

            var span = thumbColor.Value.Source switch
            {
                XAttribute attr => LocationHelpers.GetAttributeSpan(attr, context.Source),
                XElement elem => LocationHelpers.GetElementNameSpan(elem),
                _ => throw new InvalidOperationException(
                    "TryGetValueAndSource must return an XAttribute or XElement source."),
            };

            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Slider.ThumbImageSource takes precedence over ThumbColor; ThumbColor has no effect when both are set.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
