using XamlLint.Core.Helpers;
using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Rules.Bindings;

[XamlRule(
    Id = "LX0202",
    UpstreamId = "RXT163",
    Title = "Binding ElementName target does not exist",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0202.md")]
public sealed partial class LX0202_DanglingBindingElementName : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (!ElementReference.TryParse(attr.Value, out var reference)) continue;
                if (reference.Kind != ElementReferenceKind.BindingElementName) continue;
                if (context.Names.IsDefinedInScopeOf(element, reference.TargetName)) continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"Binding ElementName='{reference.TargetName}' does not refer to any named element in scope.",
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
