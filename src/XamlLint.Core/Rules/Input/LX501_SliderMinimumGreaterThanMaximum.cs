using System.Globalization;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX501",
    UpstreamId = "RXT330",
    Title = "Slider Minimum is greater than Maximum",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Wpf | Dialect.Maui,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX501.md")]
public sealed partial class LX501_SliderMinimumGreaterThanMaximum : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Slider") continue;

            var minAttr = element.Attribute("Minimum");
            var maxAttr = element.Attribute("Maximum");

            var min = NumericRangeHelpers.TryReadLiteralDouble(minAttr);
            var max = NumericRangeHelpers.TryReadLiteralDouble(maxAttr);
            if (min is null || max is null) continue;
            if (min.Value <= max.Value) continue;

            // minAttr is guaranteed non-null when min has a value (helper returns null for null input).
            var span = LocationHelpers.GetAttributeSpan(minAttr!, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"Slider Minimum=\"{min.Value.ToString(CultureInfo.InvariantCulture)}\" is greater than Maximum=\"{max.Value.ToString(CultureInfo.InvariantCulture)}\"; the range is empty.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
