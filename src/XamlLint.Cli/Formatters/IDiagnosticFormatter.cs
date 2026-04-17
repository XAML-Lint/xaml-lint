using XamlLint.Core;

namespace XamlLint.Cli.Formatters;

public interface IDiagnosticFormatter
{
    /// <summary>
    /// Writes diagnostics to the given writer. Does not close the writer.
    /// </summary>
    void Write(TextWriter writer, IReadOnlyList<Diagnostic> diagnostics, string toolVersion);
}
