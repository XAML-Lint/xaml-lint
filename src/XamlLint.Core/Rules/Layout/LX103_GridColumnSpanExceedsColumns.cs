using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Layout;

[XamlRule(
    Id = "LX103",
    UpstreamId = "RXT104",
    Title = "Grid.ColumnSpan exceeds available columns",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX103.md")]
public sealed partial class LX103_GridColumnSpanExceedsColumns : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            // Skip property-element pseudo-nodes like <Grid.ColumnDefinitions>; they are not
            // layout children and never carry attached properties.
            if (element.Name.LocalName.Contains('.')) continue;

            var read = GridAncestryHelpers.TryReadIntegerAttachedProperty(element, "Grid.ColumnSpan");
            if (read is null) continue;

            var grid = GridAncestryHelpers.FindNearestGridAncestor(element);
            if (grid is null) continue;

            var shorthandSupported = DialectFeatures.SupportsGridDefinitionShorthand(
                context.Dialect, context.FrameworkMajorVersion);
            var columnCount = GridAncestryHelpers.CountColumnDefinitions(grid, shorthandSupported);
            var spanValue = read.Value.Value;
            // Flag only when span alone exceeds total columns. Span == columnCount is legal
            // (element spans the whole Grid); column + span overflow is out of scope.
            if (spanValue <= columnCount) continue;

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
                Message: $"Grid.ColumnSpan=\"{spanValue}\" exceeds the enclosing Grid's {columnCount} column{(columnCount == 1 ? "" : "s")}.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
