using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Naming;

[XamlRule(
    Id = "LX301",
    UpstreamId = "RXT451",
    Title = "x:Uid should start with uppercase",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Uwp | Dialect.WinUI3,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX301.md")]
public sealed partial class LX301_XUidCasing : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (attr.Name.LocalName != "Uid") continue;
                if (!XamlNamespaces.IsXamlNamespace(attr.Name.NamespaceName)) continue;

                var value = attr.Value;
                if (value.Length == 0) continue;
                if (char.IsUpper(value[0])) continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: $"x:Uid '{value}' should start with an uppercase letter.",
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
