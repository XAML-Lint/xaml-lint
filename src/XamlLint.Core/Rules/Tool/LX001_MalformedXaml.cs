namespace XamlLint.Core.Rules.Tool;

[XamlRule(
    Id = "LX001",
    UpstreamId = "RXT999",
    Title = "Malformed XAML",
    DefaultSeverity = Severity.Error,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX001.md")]
public sealed partial class LX001_MalformedXaml : IToolRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context) =>
        Enumerable.Empty<Diagnostic>();
}
