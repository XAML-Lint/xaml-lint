namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX002",
    Title = "Unrecognized pragma directive",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX002.md")]
public sealed partial class LX002_UnrecognizedPragma : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
