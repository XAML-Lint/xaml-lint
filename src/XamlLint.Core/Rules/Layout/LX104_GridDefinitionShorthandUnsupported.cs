using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Layout;

[XamlRule(
    Id = "LX104",
    UpstreamId = null,
    Title = "Grid definition shorthand not supported by target framework",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX104.md")]
public sealed partial class LX104_GridDefinitionShorthandUnsupported : IXamlRule
{
    private static readonly string[] ShorthandAttributeNames =
    {
        GridAncestryHelpers.RowDefinitionsShorthandAttribute,
        GridAncestryHelpers.ColumnDefinitionsShorthandAttribute,
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        // Whole-document short-circuit: when the dialect+framework combination supports
        // the shorthand, the rule is a no-op.
        if (DialectFeatures.SupportsGridDefinitionShorthand(context.Dialect, context.FrameworkMajorVersion))
            yield break;

        // The else-branch is defensive: every "shorthand unsupported" combination today implies
        // a non-null FrameworkMajorVersion (WPF + version < 10). The fallback covers a future
        // dialect that might mark shorthand as unsupported regardless of version.
        var frameworkLabel = context.FrameworkMajorVersion is { } v
            ? $"{context.Dialect} .NET {v}"
            : context.Dialect.ToString();

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != GridAncestryHelpers.GridElementName) continue;

            foreach (var attrName in ShorthandAttributeNames)
            {
                var attr = element.Attribute(attrName);
                if (attr is null) continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"Grid definition shorthand '{attrName}' is not supported on {frameworkLabel}; use <Grid.{attrName}> with explicit definition children instead.",
                    File: document.FilePath,
                    StartLine: span.StartLine,
                    StartCol: span.StartCol,
                    EndLine: span.EndLine,
                    EndCol: span.EndCol,
                    HelpUri: Metadata.HelpUri);
            }
        }
    }
}
