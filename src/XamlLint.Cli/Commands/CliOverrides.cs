using XamlLint.Core;

namespace XamlLint.Cli.Commands;

/// <summary>
/// Parsed CLI overlay applied on top of config-resolved severities.
/// <see cref="PresetOverride"/> null → honour config's <c>extends:</c>; non-null replaces it
/// (use <c>xaml-lint:off</c>/<c>xaml-lint:recommended</c>/<c>xaml-lint:strict</c>).
/// Each <see cref="RuleSeverities"/> entry overlays the per-file severity map:
/// null value → remove (rule off); non-null → set that severity.
/// </summary>
public sealed record CliOverrides(
    string? PresetOverride,
    IReadOnlyDictionary<string, Severity?> RuleSeverities,
    bool NoInlineConfig)
{
    public static readonly CliOverrides Empty = new(
        PresetOverride: null,
        RuleSeverities: new Dictionary<string, Severity?>(0),
        NoInlineConfig: false);
}
