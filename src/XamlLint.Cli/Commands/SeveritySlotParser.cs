using XamlLint.Core;

namespace XamlLint.Cli.Commands;

/// <summary>
/// Parses a severity-slot token. A null <paramref name="severity"/> on success = "off"
/// (rule removed from the severity map). Matches the vocabulary used in
/// <c>xaml-lint.config.json</c>'s <c>rules:</c> block.
/// </summary>
internal static class SeveritySlotParser
{
    public static bool TryParse(string raw, out Severity? severity, out string error)
    {
        error = string.Empty;
        severity = null;
        switch ((raw ?? string.Empty).ToLowerInvariant())
        {
            case "off":     severity = null;             return true;
            case "info":    severity = Severity.Info;    return true;
            case "warning": severity = Severity.Warning; return true;
            case "error":   severity = Severity.Error;   return true;
            default:
                error = $"unknown severity '{raw}' (expected off|info|warning|error).";
                return false;
        }
    }
}
