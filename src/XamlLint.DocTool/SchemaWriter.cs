using System.Text.Json;
using XamlLint.Core;

namespace XamlLint.DocTool;

public sealed record SchemaAction(string Path, string ExpectedContent, bool Changed);

public static class SchemaWriter
{
    public static SchemaAction Run(string repoRoot, bool checkOnly)
    {
        var target = Path.Combine(repoRoot, "schema", "v1", "config.json");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        var schema = BuildSchema(GeneratedRuleCatalog.Rules.Select(r => r.Metadata.Id).OrderBy(s => s, StringComparer.Ordinal).ToList());

        var current = File.Exists(target) ? File.ReadAllText(target) : string.Empty;
        var changed = !string.Equals(current, schema, StringComparison.Ordinal);

        if (changed && !checkOnly)
            File.WriteAllText(target, schema);

        return new SchemaAction(target, schema, changed);
    }

    private static string BuildSchema(IReadOnlyList<string> ruleIds)
    {
        using var buffer = new MemoryStream();
        using (var w = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
        {
            w.WriteStartObject();
            w.WriteString("$schema", "https://json-schema.org/draft/2020-12/schema");
            w.WriteString("$id", "https://raw.githubusercontent.com/jizc/xaml-lint/main/schema/v1/config.json");
            w.WriteString("title", "xaml-lint configuration");
            w.WriteString("type", "object");

            w.WriteStartObject("properties");

            w.WriteStartObject("$schema");
            w.WriteString("type", "string");
            w.WriteEndObject();

            w.WriteStartObject("extends");
            w.WriteString("type", "string");
            w.WriteStartArray("enum");
            w.WriteStringValue("xaml-lint:off");
            w.WriteStringValue("xaml-lint:recommended");
            w.WriteStringValue("xaml-lint:strict");
            w.WriteEndArray();
            w.WriteEndObject();

            w.WriteStartObject("defaultDialect");
            w.WriteStartArray("enum");
            foreach (var d in new[] { "wpf", "winui3", "uwp", "maui", "avalonia", "uno" })
                w.WriteStringValue(d);
            w.WriteEndArray();
            w.WriteEndObject();

            w.WriteStartObject("overrides");
            w.WriteString("type", "array");
            w.WriteEndObject();

            w.WriteStartObject("rules");
            w.WriteString("type", "object");
            w.WriteStartObject("propertyNames");
            w.WriteStartArray("enum");
            w.WriteStringValue("*");
            foreach (var id in ruleIds) w.WriteStringValue(id);
            w.WriteEndArray();
            w.WriteEndObject();
            w.WriteEndObject();

            w.WriteEndObject(); // properties
            w.WriteEndObject();
        }
        return System.Text.Encoding.UTF8.GetString(buffer.ToArray()) + "\n";
    }
}
