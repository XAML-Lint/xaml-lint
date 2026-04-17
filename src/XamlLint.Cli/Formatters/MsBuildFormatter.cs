using XamlLint.Core;

namespace XamlLint.Cli.Formatters;

public sealed class MsBuildFormatter : IDiagnosticFormatter
{
    public void Write(TextWriter writer, IReadOnlyList<Diagnostic> diagnostics, string toolVersion)
    {
        foreach (var d in diagnostics)
        {
            var severity = d.Severity switch
            {
                Severity.Info => "info",
                Severity.Warning => "warning",
                Severity.Error => "error",
                _ => "warning",
            };

            if (d.HelpUri is null)
                writer.WriteLine($"{d.File}({d.StartLine},{d.StartCol}): {severity} {d.RuleId}: {d.Message}");
            else
                writer.WriteLine($"{d.File}({d.StartLine},{d.StartCol}): {severity} {d.RuleId}: {d.Message} [{d.HelpUri}]");
        }
    }
}
