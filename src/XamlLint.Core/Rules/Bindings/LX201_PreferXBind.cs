using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Bindings;

[XamlRule(
    Id = "LX201",
    UpstreamId = "RXT170",
    Title = "Prefer x:Bind over Binding",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.Uwp | Dialect.WinUI3 | Dialect.Uno,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX201.md")]
public sealed partial class LX201_PreferXBind : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (!MarkupExtensionHelpers.TryParseExtension(attr.Value, out var ext)) continue;
                if (ext.Name != "Binding") continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: "Prefer {x:Bind} over {Binding}; compiled bindings are faster and validated at build time.",
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
