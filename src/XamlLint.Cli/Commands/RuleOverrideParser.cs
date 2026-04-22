using System.Text.Json;
using XamlLint.Core;

namespace XamlLint.Cli.Commands;

internal sealed record RuleOverrideParseResult(
    IReadOnlyDictionary<string, Severity?> Severities,
    IReadOnlyList<string> Errors);

/// <summary>
/// Parses a sequence of <c>--rule</c> argument values. Each value is either:
///   * short form  "ID:severity" (optionally CSV-stacked, e.g. "LX100:warning,LX200:off")
///   * object form '{ "ID": "severity", ... }' discriminated by a leading '{'
/// Later values overwrite earlier ones. Unknown rule IDs and malformed tokens produce
/// human-readable error strings; the caller surfaces them via System.CommandLine.
/// </summary>
internal static class RuleOverrideParser
{
    public static RuleOverrideParseResult Parse(
        IReadOnlyList<string> values,
        IReadOnlyList<string> catalogRuleIds)
    {
        var result = new Dictionary<string, Severity?>(StringComparer.Ordinal);
        var errors = new List<string>();
        var catalog = new HashSet<string>(catalogRuleIds, StringComparer.Ordinal);

        foreach (var raw in values)
        {
            var trimmed = (raw ?? string.Empty).Trim();
            if (trimmed.Length == 0) continue;

            if (trimmed[0] == '{') ParseObject(trimmed, catalog, result, errors);
            else ParseShortCsv(trimmed, catalog, result, errors);
        }

        return new RuleOverrideParseResult(result, errors);
    }

    private static void ParseShortCsv(
        string csv,
        HashSet<string> catalog,
        Dictionary<string, Severity?> result,
        List<string> errors)
    {
        foreach (var piece in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var colon = piece.IndexOf(':');
            if (colon < 0)
            {
                errors.Add($"--rule '{piece}': missing severity — expected 'ID:<severity>' (off|info|warning|error).");
                continue;
            }

            var id = piece[..colon].Trim();
            var sevRaw = piece[(colon + 1)..].Trim();

            if (!catalog.Contains(id))
            {
                errors.Add($"--rule '{piece}': unknown rule ID '{id}'.");
                continue;
            }

            if (!SeveritySlotParser.TryParse(sevRaw, out var sev, out var err))
            {
                errors.Add($"--rule '{piece}': {err}");
                continue;
            }

            result[id] = sev;
        }
    }

    private static void ParseObject(
        string json,
        HashSet<string> catalog,
        Dictionary<string, Severity?> result,
        List<string> errors)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch (JsonException ex)
        {
            errors.Add($"--rule object form is not valid JSON: {ex.Message}");
            return;
        }

        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                errors.Add("--rule object form must be a JSON object of rule-id → severity.");
                return;
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (!catalog.Contains(prop.Name))
                {
                    errors.Add($"--rule object form: unknown rule ID '{prop.Name}'.");
                    continue;
                }

                if (prop.Value.ValueKind != JsonValueKind.String)
                {
                    errors.Add($"--rule object form: rule '{prop.Name}' must be a string severity.");
                    continue;
                }

                if (!SeveritySlotParser.TryParse(prop.Value.GetString()!, out var sev, out var err))
                {
                    errors.Add($"--rule object form: rule '{prop.Name}': {err}");
                    continue;
                }

                result[prop.Name] = sev;
            }
        }
    }
}
