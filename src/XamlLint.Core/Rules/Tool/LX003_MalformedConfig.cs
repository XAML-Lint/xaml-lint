namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX003",
    Title = "Malformed configuration",
    DefaultSeverity = Severity.Error,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX003.md")]
public sealed partial class LX003_MalformedConfig : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
