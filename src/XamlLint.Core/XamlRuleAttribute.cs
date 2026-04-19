namespace XamlLint.Core;

/// <summary>
/// Declares rule metadata on an <see cref="IXamlRule"/> implementation. The source generator
/// (<c>XamlLint.Core.SourceGen</c>) reads this attribute at build time to emit the catalog
/// and each class's <c>Metadata</c> property.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class XamlRuleAttribute : Attribute
{
    public required string Id { get; init; }
    public string? UpstreamId { get; init; }
    public required string Title { get; init; }
    public required Severity DefaultSeverity { get; init; }
    public required Dialect Dialects { get; init; }
    public required string HelpUri { get; init; }
    public bool Deprecated { get; init; }
    public string? ReplacedBy { get; init; }

    /// <summary>
    /// Whether the rule is enabled in the <c>xaml-lint:recommended</c> preset (and therefore
    /// also in the no-config fallback, which uses <c>:recommended</c>). Default <c>true</c>;
    /// set to <c>false</c> for rules whose signal is valuable but too noisy for most users
    /// out-of-the-box (e.g. localization, style-preference). Off-by-default rules still
    /// appear in <c>:strict</c> at the usual escalated severity, and users who extend
    /// <c>:recommended</c> can enable them explicitly.
    /// </summary>
    public bool DefaultEnabled { get; init; } = true;
}
