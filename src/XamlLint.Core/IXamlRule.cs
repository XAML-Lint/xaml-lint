namespace XamlLint.Core;

/// <summary>
/// Stateless rule contract. Every rule class implements this once; the source generator
/// fills in <see cref="Metadata"/> from the <see cref="XamlRuleAttribute"/>.
/// </summary>
public interface IXamlRule
{
    RuleMetadata Metadata { get; }

    IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context);
}
