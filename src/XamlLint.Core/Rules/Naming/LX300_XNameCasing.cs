using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Naming;

[XamlRule(
    Id = "LX300",
    UpstreamId = "RXT452",
    Title = "x:Name should start with uppercase",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX300.md")]
public sealed partial class LX300_XNameCasing : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (attr.Name.LocalName != "Name") continue;
                if (!XamlNamespaces.IsXamlNamespace(attr.Name.NamespaceName)) continue;

                var value = attr.Value;
                if (value.Length == 0) continue;
                if (char.IsUpper(value[0])) continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"x:Name '{value}' should start with an uppercase letter.",
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
