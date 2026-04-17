using System.Text.Json;
using XamlLint.Core;

namespace XamlLint.DocTool;

public sealed record PresetAction(string Path, string ExpectedContent, bool Changed);

public static class PresetWriter
{
    public static IReadOnlyList<PresetAction> Run(string repoRoot, bool checkOnly)
    {
        var dir = Path.Combine(repoRoot, "schema", "v1", "presets");
        Directory.CreateDirectory(dir);

        var rules = GeneratedRuleCatalog.Rules.Select(r => r.Metadata).OrderBy(m => m.Id, StringComparer.Ordinal).ToList();

        return new[]
        {
            Write(Path.Combine(dir, "xaml-lint-off.json"),         rules, OffLevel,          checkOnly),
            Write(Path.Combine(dir, "xaml-lint-recommended.json"), rules, RecommendedLevel,  checkOnly),
            Write(Path.Combine(dir, "xaml-lint-strict.json"),      rules, StrictLevel,       checkOnly),
        };
    }

    private static PresetAction Write(string path, IReadOnlyList<RuleMetadata> rules, Func<RuleMetadata, string> level, bool checkOnly)
    {
        var expected = Build(rules, level);
        var current = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        var changed = !string.Equals(current, expected, StringComparison.Ordinal);
        if (changed && !checkOnly) File.WriteAllText(path, expected);
        return new PresetAction(path, expected, changed);
    }

    private static string Build(IReadOnlyList<RuleMetadata> rules, Func<RuleMetadata, string> level)
    {
        using var buffer = new MemoryStream();
        using (var w = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
        {
            w.WriteStartObject();
            w.WriteString("$schema", "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json");
            w.WriteStartObject("rules");
            foreach (var m in rules)
                w.WriteString(m.Id, level(m));
            w.WriteEndObject();
            w.WriteEndObject();
        }
        return System.Text.Encoding.UTF8.GetString(buffer.ToArray()) + "\n";
    }

    private static string OffLevel(RuleMetadata m) => "off";
    private static string RecommendedLevel(RuleMetadata m) => SeverityName(m.DefaultSeverity);
    private static string StrictLevel(RuleMetadata m) => m.DefaultSeverity switch
    {
        Severity.Info => "warning",
        Severity.Warning => "error",
        Severity.Error => "error",
        _ => "error",
    };

    private static string SeverityName(Severity s) => s switch
    {
        Severity.Info => "info",
        Severity.Warning => "warning",
        Severity.Error => "error",
        _ => "error",
    };
}
