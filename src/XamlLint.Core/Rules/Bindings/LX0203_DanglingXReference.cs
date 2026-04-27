using System.Xml.Linq;
using XamlLint.Core.Helpers;
using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Rules.Bindings;

[XamlRule(
    Id = "LX0203",
    Title = "x:Reference target does not exist",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0203.md")]
public sealed partial class LX0203_DanglingXReference : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            // Element-form: <x:Reference Name="Foo"/>. The Name attribute is a literal value,
            // not a markup extension. Match by local name "Reference" — the namespace check is
            // skipped here because all dialects place <x:Reference> in the XAML namespace and
            // the rule's filter on Name resolution makes a custom <foo:Reference> firing on
            // a literal "Name" attribute extremely unlikely to be a false positive.
            if (IsXReferenceElement(element))
            {
                var nameAttr = element.Attribute("Name");
                if (nameAttr is not null && !string.IsNullOrWhiteSpace(nameAttr.Value))
                {
                    if (!context.Names.IsDefinedInScopeOf(element, nameAttr.Value))
                    {
                        var span = LocationHelpers.GetAttributeSpan(nameAttr, context.Source);
                        yield return new Diagnostic(
                            RuleId: Metadata.Id,
                            Severity: Metadata.DefaultSeverity,
                            Message: $"x:Reference '{nameAttr.Value}' does not refer to any named element in scope.",
                            File: document.FilePath,
                            StartLine: span.StartLine,
                            StartCol: span.StartCol,
                            EndLine: span.EndLine,
                            EndCol: span.EndCol,
                            HelpUri: Metadata.HelpUri);
                    }
                }
            }

            // Attribute-form (existing) + nested markup-extension references via FindAll.
            foreach (var attr in element.Attributes())
            {
                foreach (var reference in ElementReference.FindAll(attr.Value))
                {
                    if (reference.Kind != ElementReferenceKind.XReference) continue;
                    if (context.Names.IsDefinedInScopeOf(element, reference.TargetName)) continue;

                    var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                    yield return new Diagnostic(
                        RuleId: Metadata.Id,
                        Severity: Metadata.DefaultSeverity,
                        Message: $"x:Reference '{reference.TargetName}' does not refer to any named element in scope.",
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

    private static bool IsXReferenceElement(XElement element) =>
        element.Name.LocalName == "Reference";
}
