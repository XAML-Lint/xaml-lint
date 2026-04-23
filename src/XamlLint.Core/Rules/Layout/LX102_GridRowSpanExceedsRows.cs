using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Layout;

[XamlRule(
    Id = "LX102",
    UpstreamId = "RXT103",
    Title = "Grid.RowSpan exceeds available rows",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX102.md")]
public sealed partial class LX102_GridRowSpanExceedsRows : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            // Skip property-element pseudo-nodes like <Grid.RowDefinitions>; they are not
            // layout children and never carry attached properties.
            if (element.Name.LocalName.Contains('.')) continue;

            var read = GridAncestryHelpers.TryReadIntegerAttachedProperty(element, "Grid.RowSpan");
            if (read is null) continue;

            var grid = GridAncestryHelpers.FindNearestGridAncestor(element);
            if (grid is null) continue;

            var shorthandSupported = DialectFeatures.SupportsGridDefinitionShorthand(
                context.Dialect, context.FrameworkMajorVersion);
            var rowCount = GridAncestryHelpers.CountRowDefinitions(grid, shorthandSupported);
            var spanValue = read.Value.Value;
            // Flag only when span alone exceeds total rows. Span == rowCount is legal
            // (element spans the whole Grid); row + span overflow is out of scope.
            if (spanValue <= rowCount) continue;

            var span = read.Value.Source switch
            {
                XAttribute attr => LocationHelpers.GetAttributeSpan(attr, context.Source),
                XElement el => LocationHelpers.GetElementNameSpan(el),
                _ => throw new InvalidOperationException(
                    $"Unexpected attached-property source type {read.Value.Source.GetType().Name}.")
            };

            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"Grid.RowSpan=\"{spanValue}\" exceeds the enclosing Grid's {rowCount} row{(rowCount == 1 ? "" : "s")}.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
