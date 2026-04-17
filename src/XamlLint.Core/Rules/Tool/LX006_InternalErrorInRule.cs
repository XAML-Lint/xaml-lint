namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX006",
    Title = "Internal error in rule",
    DefaultSeverity = Severity.Error,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX006.md")]
public sealed partial class LX006_InternalErrorInRule : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
