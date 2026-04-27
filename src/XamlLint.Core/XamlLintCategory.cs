namespace XamlLint.Core;

/// <summary>
/// Rule category derived from the ID's hundreds digit (per design §3.5).
/// </summary>
public enum XamlLintCategory
{
    Tool,          // LX0001-LX0099
    Layout,        // LX0100-LX0199
    Bindings,      // LX0200-LX0299
    Naming,        // LX0300-LX0399
    Resources,     // LX0400-LX0499
    Input,         // LX0500-LX0599
    Usability,     // LX0600-LX0699
    Accessibility, // LX0700-LX0799
    Platform,      // LX0800-LX0899
}

public static class XamlLintCategoryExtensions
{
    /// <summary>
    /// Maps a rule ID like <c>LX0100</c> to its category by looking at the hundreds digit.
    /// Throws <see cref="ArgumentException"/> for malformed IDs.
    /// </summary>
    public static XamlLintCategory ForId(string ruleId)
    {
        if (string.IsNullOrEmpty(ruleId) || !ruleId.StartsWith("LX", StringComparison.Ordinal) || ruleId.Length != 6)
        {
            throw new ArgumentException($"Rule ID must match the pattern 'LX####' (4 digits); got '{ruleId}'.", nameof(ruleId));
        }

        if (!int.TryParse(ruleId.AsSpan(2), out var number))
        {
            throw new ArgumentException($"Rule ID '{ruleId}' must have 4 digits after 'LX'.", nameof(ruleId));
        }

        return (number / 100) switch
        {
            0 => XamlLintCategory.Tool,
            1 => XamlLintCategory.Layout,
            2 => XamlLintCategory.Bindings,
            3 => XamlLintCategory.Naming,
            4 => XamlLintCategory.Resources,
            5 => XamlLintCategory.Input,
            6 => XamlLintCategory.Usability,
            7 => XamlLintCategory.Accessibility,
            8 => XamlLintCategory.Platform,
            _ => throw new ArgumentException($"Rule ID '{ruleId}' is outside the LX0001-LX0899 range currently defined.", nameof(ruleId)),
        };
    }
}

public static class XamlLintCategoryNames
{
    // Stable wire-format names used in AnalyzerReleases and SARIF tool metadata.
    public static string NameOf(XamlLintCategory c) => c switch
    {
        XamlLintCategory.Tool          => "Tool",
        XamlLintCategory.Layout        => "Layout",
        XamlLintCategory.Bindings      => "Bindings",
        XamlLintCategory.Naming        => "Naming",
        XamlLintCategory.Resources     => "Resources",
        XamlLintCategory.Input         => "Input",
        XamlLintCategory.Usability     => "Usability",
        XamlLintCategory.Accessibility => "Accessibility",
        XamlLintCategory.Platform      => "Platform",
        _ => throw new ArgumentOutOfRangeException(nameof(c)),
    };
}
