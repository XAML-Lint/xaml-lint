using XamlLint.Core;

namespace XamlLint.Cli.Formatters;

public sealed class PrettyFormatter(bool useColor) : IDiagnosticFormatter
{
    private const string Reset = "\u001b[0m";
    private const string Red = "\u001b[31m";
    private const string Yellow = "\u001b[33m";
    private const string Cyan = "\u001b[36m";
    private const string Bold = "\u001b[1m";
    private const string Dim = "\u001b[2m";

    public static bool ShouldUseColor()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"))) return false;
        return !Console.IsOutputRedirected;
    }

    public void Write(TextWriter writer, IReadOnlyList<Diagnostic> diagnostics, string toolVersion)
    {
        _ = toolVersion;

        if (diagnostics.Count == 0)
        {
            writer.WriteLine("No issues found.");
            return;
        }

        var grouped = diagnostics.GroupBy(d => d.File).OrderBy(g => g.Key, StringComparer.Ordinal);
        foreach (var group in grouped)
        {
            writer.WriteLine(C(Bold) + group.Key + C(Reset));
            foreach (var d in group.OrderBy(d => d.StartLine).ThenBy(d => d.StartCol))
            {
                writer.WriteLine(
                    $"  {C(Dim)}{d.StartLine,4}:{d.StartCol,-3}{C(Reset)} " +
                    $"{SeverityTag(d.Severity)} " +
                    $"{C(Cyan)}{d.RuleId}{C(Reset)}  " +
                    $"{d.Message}");
            }
            writer.WriteLine();
        }
    }

    private string SeverityTag(Severity s) => s switch
    {
        Severity.Error => C(Red) + "error  " + C(Reset),
        Severity.Warning => C(Yellow) + "warning" + C(Reset),
        Severity.Info => C(Dim) + "info   " + C(Reset),
        _ => "       ",
    };

    private string C(string code) => useColor ? code : string.Empty;
}
