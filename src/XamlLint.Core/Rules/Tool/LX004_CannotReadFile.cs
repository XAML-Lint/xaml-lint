namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX004",
    Title = "Cannot read file",
    DefaultSeverity = Severity.Error,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX004.md")]
public sealed partial class LX004_CannotReadFile : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
