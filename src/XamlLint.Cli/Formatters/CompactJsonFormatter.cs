using System.Text.Json;
using XamlLint.Core;

namespace XamlLint.Cli.Formatters;

public sealed class CompactJsonFormatter : IDiagnosticFormatter
{
    private static readonly JsonWriterOptions WriterOptions = new() { Indented = true };

    public void Write(TextWriter writer, IReadOnlyList<Diagnostic> diagnostics, string toolVersion)
    {
        using var buffer = new MemoryStream();
        using (var w = new Utf8JsonWriter(buffer, WriterOptions))
        {
            w.WriteStartObject();
            w.WriteString("version", "1");

            w.WriteStartObject("tool");
            w.WriteString("name", "xaml-lint");
            w.WriteString("version", toolVersion);
            w.WriteEndObject();

            w.WriteStartArray("results");
            foreach (var d in diagnostics)
            {
                w.WriteStartObject();
                w.WriteString("file", d.File);
                w.WriteString("ruleId", d.RuleId);
                w.WriteString("severity", SeverityName(d.Severity));
                w.WriteString("message", d.Message);
                w.WriteNumber("startLine", d.StartLine);
                w.WriteNumber("startCol", d.StartCol);
                w.WriteNumber("endLine", d.EndLine);
                w.WriteNumber("endCol", d.EndCol);
                if (d.HelpUri is not null) w.WriteString("helpUri", d.HelpUri);
                w.WriteEndObject();
            }
            w.WriteEndArray();

            w.WriteEndObject();
        }

        writer.Write(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    }

    private static string SeverityName(Severity s) => s switch
    {
        Severity.Info => "info",
        Severity.Warning => "warning",
        Severity.Error => "error",
        _ => throw new ArgumentOutOfRangeException(nameof(s)),
    };
}
