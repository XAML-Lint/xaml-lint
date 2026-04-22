using XamlLint.Core;

namespace XamlLint.Cli.Commands;

internal sealed record OnlyExpansionResult(
    string PresetOverride,
    bool ForceNoConfigLookup,
    IReadOnlyDictionary<string, Severity?> Severities,
    IReadOnlyList<string> Errors);

/// <summary>
/// Parse-time desugaring of <c>--only ID[:sev][,ID[:sev]...]</c>. Equivalent to
/// <c>--preset none --no-config-lookup --rule ID:sev ...</c>. Bare IDs use the rule's
/// <c>DefaultSeverity</c>. <c>off</c> is not permitted in --only.
/// </summary>
internal static class OnlyExpansion
{
    public static OnlyExpansionResult Expand(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, Severity> defaultSeverities)
    {
        var result = new Dictionary<string, Severity?>(StringComparer.Ordinal);
        var errors = new List<string>();

        foreach (var raw in values)
        {
            foreach (var piece in (raw ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var colon = piece.IndexOf(':');
                var id = colon < 0 ? piece : piece[..colon].Trim();
                if (!defaultSeverities.ContainsKey(id))
                {
                    errors.Add($"--only '{piece}': unknown rule ID '{id}'.");
                    continue;
                }

                if (colon < 0)
                {
                    result[id] = defaultSeverities[id];
                    continue;
                }

                var sevRaw = piece[(colon + 1)..].Trim();
                if (string.Equals(sevRaw, "off", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"--only '{piece}': 'off' is not allowed in --only (use --rule ID:off).");
                    continue;
                }

                if (!SeveritySlotParser.TryParse(sevRaw, out var sev, out var err))
                {
                    errors.Add($"--only '{piece}': {err}");
                    continue;
                }

                result[id] = sev!.Value;
            }
        }

        return new OnlyExpansionResult(
            PresetOverride: "xaml-lint:off",
            ForceNoConfigLookup: true,
            Severities: result,
            Errors: errors);
    }
}
