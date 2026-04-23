namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX0005",
    Title = "Skipping non-XAML file",
    DefaultSeverity = Severity.Info,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0005.md")]
public sealed partial class LX0005_SkippingNonXaml : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
