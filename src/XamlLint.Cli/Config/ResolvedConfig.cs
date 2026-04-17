namespace XamlLint.Cli.Config;

/// <summary>
/// Post-resolution view of config: effective severity per rule ID, resolved default dialect,
/// and optional per-file dialect overrides.
/// </summary>
public sealed record ResolvedConfig(
    IReadOnlyDictionary<string, XamlLint.Core.Severity> RuleSeverities,
    XamlLint.Core.Dialect DefaultDialect,
    IReadOnlyList<ResolvedOverride> Overrides,
    string? SourcePath);

/// <summary>
/// Per-file override. <see cref="RuleSeverities"/> values may be <c>null</c> to indicate
/// the rule is turned off for matching files (removed from the effective map).
/// </summary>
public sealed record ResolvedOverride(
    string FilesGlob,
    XamlLint.Core.Dialect? Dialect,
    IReadOnlyDictionary<string, XamlLint.Core.Severity?> RuleSeverities);
