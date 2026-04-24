using System.Xml.Linq;
using XamlLint.Core.Helpers;
using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Rules.Naming;

[XamlRule(
    Id = "LX0302",
    Title = "Unused x:Name",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0302.md")]
public sealed partial class LX0302_UnusedXName : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        var used = XamlNameReferenceScanner.ComputeUsedDeclarations(document.Root, context.Names);

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            var nameAttr = FindXNameAttribute(element);
            if (nameAttr is null) continue;
            if (string.IsNullOrWhiteSpace(nameAttr.Value)) continue;
            if (used.Contains(element)) continue;

            var span = LocationHelpers.GetAttributeSpan(nameAttr, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"x:Name '{nameAttr.Value}' is declared but not referenced in this XAML file.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static XAttribute? FindXNameAttribute(XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            if (attr.Name.LocalName != "Name") continue;
            if (!XamlNamespaces.IsXamlNamespace(attr.Name.NamespaceName)) continue;
            return attr;
        }
        return null;
    }
}
