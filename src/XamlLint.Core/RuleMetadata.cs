namespace XamlLint.Core;

/// <summary>
/// Compile-time metadata about a rule. Populated by the source generator from the
/// <see cref="XamlRuleAttribute"/> applied to each <see cref="IXamlRule"/> implementation.
/// </summary>
public sealed record RuleMetadata(
    string Id,
    string? UpstreamId,
    string Title,
    Severity DefaultSeverity,
    Dialect Dialects,
    string HelpUri,
    bool Deprecated,
    string? ReplacedBy,
    bool DefaultEnabled = true);
