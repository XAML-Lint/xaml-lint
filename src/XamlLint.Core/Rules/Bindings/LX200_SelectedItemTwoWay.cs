using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Bindings;

[XamlRule(
    Id = "LX200",
    UpstreamId = "RXT160",
    Title = "SelectedItem binding should be TwoWay",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX200.md")]
public sealed partial class LX200_SelectedItemTwoWay : IXamlRule
{
    private static readonly HashSet<string> BindingExtensions = new(StringComparer.Ordinal)
    {
        "Binding",
        "x:Bind",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                if (attr.Name.LocalName != "SelectedItem") continue;
                if (!MarkupExtensionHelpers.TryParseExtension(attr.Value, out var ext)) continue;
                if (!BindingExtensions.Contains(ext.Name)) continue;
                if (ext.NamedArguments.TryGetValue("Mode", out var mode) && mode == "TwoWay") continue;

                var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
                yield return new Diagnostic(
                    RuleId: Metadata.Id,
                    Severity: Metadata.DefaultSeverity,
                    Message: "SelectedItem binding should explicitly set Mode=TwoWay.",
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
