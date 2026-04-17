namespace XamlLint.Core;

/// <summary>
/// Rule category derived from the ID's hundreds digit (per design §3.5).
/// </summary>
public enum XamlLintCategory
{
    Tool,        // LX001-LX099
    Layout,      // LX100-LX199
    Bindings,    // LX200-LX299
    Naming,      // LX300-LX399
    Resources,   // LX400-LX499
    Input,       // LX500-LX599
    Deprecated,  // LX600-LX699
}

public static class XamlLintCategoryExtensions
{
    /// <summary>
    /// Maps a rule ID like <c>LX100</c> to its category by looking at the hundreds digit.
    /// Throws <see cref="ArgumentException"/> for malformed IDs.
    /// </summary>
    public static XamlLintCategory ForId(string ruleId)
    {
        if (string.IsNullOrEmpty(ruleId) || !ruleId.StartsWith("LX", StringComparison.Ordinal) || ruleId.Length != 5)
        {
            throw new ArgumentException($"Rule ID must match the pattern 'LX###' (3 digits); got '{ruleId}'.", nameof(ruleId));
        }

        if (!int.TryParse(ruleId.AsSpan(2), out var number))
        {
            throw new ArgumentException($"Rule ID '{ruleId}' must have 3 digits after 'LX'.", nameof(ruleId));
        }

        return (number / 100) switch
        {
            0 => XamlLintCategory.Tool,
            1 => XamlLintCategory.Layout,
            2 => XamlLintCategory.Bindings,
            3 => XamlLintCategory.Naming,
            4 => XamlLintCategory.Resources,
            5 => XamlLintCategory.Input,
            6 => XamlLintCategory.Deprecated,
            _ => throw new ArgumentException($"Rule ID '{ruleId}' is outside the LX001-LX699 range currently defined.", nameof(ruleId)),
        };
    }
}

public static class XamlLintCategoryNames
{
    // Stable wire-format names used in AnalyzerReleases and SARIF tool metadata.
    public static string NameOf(XamlLintCategory c) => c switch
    {
        XamlLintCategory.Tool       => "Tool",
        XamlLintCategory.Layout     => "Layout",
        XamlLintCategory.Bindings   => "Bindings",
        XamlLintCategory.Naming     => "Naming",
        XamlLintCategory.Resources  => "Resources",
        XamlLintCategory.Input      => "Input",
        XamlLintCategory.Deprecated => "Deprecated",
        _ => throw new ArgumentOutOfRangeException(nameof(c)),
    };
}
