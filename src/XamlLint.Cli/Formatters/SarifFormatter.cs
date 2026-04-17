using System.Text.Json;
using XamlLint.Core;

namespace XamlLint.Cli.Formatters;

public sealed class SarifFormatter : IDiagnosticFormatter
{
    private const string Schema = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/main/Schemata/sarif-schema-2.1.0.json";
    private const string InfoUri = "https://github.com/jizc/xaml-lint";

    public void Write(TextWriter writer, IReadOnlyList<Diagnostic> diagnostics, string toolVersion) =>
        Write(writer, diagnostics, toolVersion, Array.Empty<Diagnostic>());

    public void Write(
        TextWriter writer,
        IReadOnlyList<Diagnostic> diagnostics,
        string toolVersion,
        IReadOnlyList<Diagnostic> suppressed)
    {
        using var buffer = new MemoryStream();
        using (var w = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
        {
            w.WriteStartObject();
            w.WriteString("version", "2.1.0");
            w.WriteString("$schema", Schema);

            w.WriteStartArray("runs");
            w.WriteStartObject();

            WriteTool(w, toolVersion, diagnostics.Concat(suppressed));
            WriteResults(w, diagnostics, suppressed);

            w.WriteEndObject();
            w.WriteEndArray();
            w.WriteEndObject();
        }

        writer.Write(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    }

    private static void WriteTool(Utf8JsonWriter w, string toolVersion, IEnumerable<Diagnostic> all)
    {
        w.WriteStartObject("tool");
        w.WriteStartObject("driver");
        w.WriteString("name", "xaml-lint");
        w.WriteString("version", toolVersion);
        w.WriteString("informationUri", InfoUri);

        var metaById = GeneratedRuleCatalog.Rules.ToDictionary(r => r.Metadata.Id, r => r.Metadata);
        var uniqueIds = all.Select(d => d.RuleId).Distinct().OrderBy(s => s, StringComparer.Ordinal).ToList();
        w.WriteStartArray("rules");
        foreach (var id in uniqueIds)
        {
            var meta = metaById.GetValueOrDefault(id);
            w.WriteStartObject();
            w.WriteString("id", id);
            w.WriteString("name", meta?.Title ?? id);
            w.WriteStartObject("shortDescription");
            w.WriteString("text", meta?.Title ?? id);
            w.WriteEndObject();
            if (meta?.HelpUri is { Length: > 0 } helpUri)
                w.WriteString("helpUri", helpUri);
            w.WriteEndObject();
        }
        w.WriteEndArray();

        w.WriteEndObject(); // driver
        w.WriteEndObject(); // tool
    }

    private static void WriteResults(Utf8JsonWriter w, IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<Diagnostic> suppressed)
    {
        w.WriteStartArray("results");
        foreach (var d in diagnostics) WriteResult(w, d, suppressed: Array.Empty<Diagnostic>());
        foreach (var d in suppressed) WriteResult(w, d, suppressed: new[] { d });
        w.WriteEndArray();
    }

    private static void WriteResult(Utf8JsonWriter w, Diagnostic d, IReadOnlyList<Diagnostic> suppressed)
    {
        w.WriteStartObject();
        w.WriteString("ruleId", d.RuleId);
        w.WriteString("level", Level(d.Severity));

        w.WriteStartObject("message");
        w.WriteString("text", d.Message);
        w.WriteEndObject();

        w.WriteStartArray("locations");
        w.WriteStartObject();
        w.WriteStartObject("physicalLocation");
        w.WriteStartObject("artifactLocation");
        w.WriteString("uri", d.File.Replace('\\', '/'));
        w.WriteEndObject();
        w.WriteStartObject("region");
        w.WriteNumber("startLine", d.StartLine);
        w.WriteNumber("startColumn", d.StartCol);
        w.WriteNumber("endLine", d.EndLine);
        w.WriteNumber("endColumn", d.EndCol);
        w.WriteEndObject();
        w.WriteEndObject();
        w.WriteEndObject();
        w.WriteEndArray();

        if (suppressed.Count > 0)
        {
            w.WriteStartArray("suppressions");
            w.WriteStartObject();
            w.WriteString("kind", "inSource");
            w.WriteEndObject();
            w.WriteEndArray();
        }

        w.WriteEndObject();
    }

    private static string Level(Severity s) => s switch
    {
        Severity.Error => "error",
        Severity.Warning => "warning",
        Severity.Info => "note",
        _ => "warning",
    };
}
