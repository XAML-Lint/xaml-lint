namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX0006",
    Title = "Internal error in rule",
    DefaultSeverity = Severity.Error,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0006.md")]
public sealed partial class LX0006_InternalErrorInRule : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
