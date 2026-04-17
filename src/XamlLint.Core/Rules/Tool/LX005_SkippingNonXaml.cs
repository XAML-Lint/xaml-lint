namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX005",
    Title = "Skipping non-XAML file",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX005.md")]
public sealed partial class LX005_SkippingNonXaml : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
