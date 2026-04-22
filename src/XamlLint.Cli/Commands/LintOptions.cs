namespace XamlLint.Cli.Commands;

public enum OutputFormat { CompactJson, Sarif, MsBuild, Pretty }

public enum Verbosity { Quiet, Minimal, Normal, Detailed, Diagnostic }

public sealed record LintOptions(
    IReadOnlyList<string> Paths,
    bool ReadFromStdin,
    OutputFormat? Format,        // null = TTY-sensitive default
    string? OutputPath,          // null = stdout
    string? ConfigPath,
    bool NoConfigLookup,
    string? Dialect,
    CliOverrides Overrides,
    IReadOnlyList<string> Include,
    IReadOnlyList<string> Exclude,
    Verbosity Verbosity,
    bool Force);
