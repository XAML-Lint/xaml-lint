using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Naming;

[XamlRule(
    Id = "LX301",
    UpstreamId = "RXT451",
    Title = "x:Uid should start with uppercase",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Uwp | Dialect.WinUI3 | Dialect.Uno,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX301.md")]
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

                // UWP/WinUI resw namespace-scope form: "/ResourceFile/Key" routes the
                // lookup to a named .resw; only the segment after the final '/' is the
                // resource key, so that's what the casing convention applies to.
                var keyStart = 0;
                if (value[0] == '/')
                {
                    var lastSlash = value.LastIndexOf('/');
                    if (lastSlash == value.Length - 1) continue;
                    keyStart = lastSlash + 1;
                }

                if (char.IsUpper(value[keyStart])) continue;

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
