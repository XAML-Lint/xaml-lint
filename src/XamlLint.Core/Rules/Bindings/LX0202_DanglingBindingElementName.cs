using System.Xml.Linq;
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
            // Element-form: <Binding ElementName="Foo"/> inside MultiBinding, PriorityBinding,
            // Setter.Value, etc. ElementName is a literal attribute, not a markup extension —
            // ElementReference.FindAll wouldn't see it because the value isn't {…}-wrapped.
            if (IsBindingElement(element))
            {
                var elementNameAttr = element.Attribute("ElementName");
                if (elementNameAttr is not null && !string.IsNullOrWhiteSpace(elementNameAttr.Value))
                {
                    if (!context.Names.IsDefinedInScopeOf(element, elementNameAttr.Value))
                    {
                        var span = LocationHelpers.GetAttributeSpan(elementNameAttr, context.Source);
                        yield return new Diagnostic(
                            RuleId: Metadata.Id,
                            Severity: Metadata.DefaultSeverity,
                            Message: $"Binding ElementName='{elementNameAttr.Value}' does not refer to any named element in scope.",
                            File: document.FilePath,
                            StartLine: span.StartLine,
                            StartCol: span.StartCol,
                            EndLine: span.EndLine,
                            EndCol: span.EndCol,
                            HelpUri: Metadata.HelpUri);
                    }
                }
            }

            // Attribute-form (existing path) + nested markup-extension references inside
            // attribute values. FindAll surfaces every reference, including nested ones.
            foreach (var attr in element.Attributes())
            {
                foreach (var reference in ElementReference.FindAll(attr.Value))
                {
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

    private static bool IsBindingElement(XElement element) =>
        element.Name.LocalName == "Binding";
}
