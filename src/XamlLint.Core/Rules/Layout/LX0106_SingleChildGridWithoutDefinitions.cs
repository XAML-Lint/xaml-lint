using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Layout;

[XamlRule(
    Id = "LX0106",
    UpstreamId = null,
    Title = "Single-child Grid without row or column definitions",
    DefaultSeverity = Severity.Warning,
    DefaultEnabled = false,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0106.md")]
public sealed partial class LX0106_SingleChildGridWithoutDefinitions : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != GridAncestryHelpers.GridElementName) continue;

            if (GridAncestryHelpers.HasDeclaredRowDefinitions(element)) continue;
            if (GridAncestryHelpers.HasDeclaredColumnDefinitions(element)) continue;

            var layoutChildCount = 0;
            foreach (var child in element.Elements())
            {
                // Property-element nodes like <Grid.Resources>, <Grid.Style> are not layout
                // children. They carry no Grid.Row / Grid.Column placement.
                if (child.Name.LocalName.Contains('.')) continue;
                layoutChildCount++;
                if (layoutChildCount > 1) break;
            }

            if (layoutChildCount != 1) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Grid has a single layout child and no row or column definitions; the Grid adds no layout and the child can replace it.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
