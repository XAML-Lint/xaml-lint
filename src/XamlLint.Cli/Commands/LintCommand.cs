using System.CommandLine;

namespace XamlLint.Cli.Commands;

internal static class LintCommand
{
    public static Command Build()
    {
        var pathsArg = new Argument<string[]>("paths")
        {
            Description = "One or more paths or globs. '-' reads newline-separated paths from stdin.",
            Arity = ArgumentArity.ZeroOrMore,
        };

        var formatOpt = new Option<string?>("--format")
        {
            Description = "Output format: compact-json | sarif | msbuild | pretty. Default depends on TTY.",
        };
        var outputOpt = new Option<string?>("--output", "-o")
        {
            Description = "Write to a file instead of stdout. '-' means stdout.",
        };
        var configOpt = new Option<string?>("--config")
        {
            Description = "Explicit config file; disables discovery.",
        };
        var noConfigOpt = new Option<bool>("--no-config")
        {
            Description = "Skip config discovery entirely; use built-in defaults.",
        };
        var dialectOpt = new Option<string?>("--dialect")
        {
            Description = "Force dialect; overrides config.",
        };
        var onlyOpt = new Option<string?>("--only")
        {
            Description = "Allow-list: comma-separated rule IDs to run (e.g. LX100,LX101).",
        };
        var includeOpt = new Option<string[]>("--include")
        {
            Description = "Include glob (repeatable). Applied after positional expansion.",
        };
        var excludeOpt = new Option<string[]>("--exclude")
        {
            Description = "Exclude glob (repeatable). Wins over --include when both match.",
        };
        var verbosityOpt = new Option<string?>("--verbosity", "-v")
        {
            Description = "q(uiet) | m(inimal) | n(ormal) | d(etailed) | diag(nostic). Default: normal.",
        };
        var forceOpt = new Option<bool>("--force")
        {
            Description = "Lint files whose extension isn't .xaml.",
        };

        var cmd = new Command("lint", "Lint XAML files and report diagnostics.");
        cmd.Arguments.Add(pathsArg);
        cmd.Options.Add(formatOpt);
        cmd.Options.Add(outputOpt);
        cmd.Options.Add(configOpt);
        cmd.Options.Add(noConfigOpt);
        cmd.Options.Add(dialectOpt);
        cmd.Options.Add(onlyOpt);
        cmd.Options.Add(includeOpt);
        cmd.Options.Add(excludeOpt);
        cmd.Options.Add(verbosityOpt);
        cmd.Options.Add(forceOpt);

        cmd.SetAction((parseResult, _) =>
        {
            var paths = parseResult.GetValue(pathsArg) ?? Array.Empty<string>();
            var opts = new LintOptions(
                Paths: paths,
                ReadFromStdin: paths.Contains("-"),
                Format: ParseFormat(parseResult.GetValue(formatOpt)),
                OutputPath: parseResult.GetValue(outputOpt),
                ConfigPath: parseResult.GetValue(configOpt),
                NoConfig: parseResult.GetValue(noConfigOpt),
                Dialect: parseResult.GetValue(dialectOpt),
                OnlyRules: SplitCsv(parseResult.GetValue(onlyOpt)),
                Include: parseResult.GetValue(includeOpt) ?? Array.Empty<string>(),
                Exclude: parseResult.GetValue(excludeOpt) ?? Array.Empty<string>(),
                Verbosity: ParseVerbosity(parseResult.GetValue(verbosityOpt)),
                Force: parseResult.GetValue(forceOpt));

            System.Console.WriteLine($"[stub] lint invoked with {opts.Paths.Count} path argument(s).");
            return Task.FromResult(0);
        });

        return cmd;
    }

    private static OutputFormat? ParseFormat(string? raw) => raw switch
    {
        null => null,
        "compact-json" => OutputFormat.CompactJson,
        "sarif" => OutputFormat.Sarif,
        "msbuild" => OutputFormat.MsBuild,
        "pretty" => OutputFormat.Pretty,
        _ => throw new ArgumentException($"Unknown --format '{raw}'."),
    };

    private static Verbosity ParseVerbosity(string? raw) => raw?.ToLowerInvariant() switch
    {
        null or "n" or "normal" => Verbosity.Normal,
        "q" or "quiet" => Verbosity.Quiet,
        "m" or "minimal" => Verbosity.Minimal,
        "d" or "detailed" => Verbosity.Detailed,
        "diag" or "diagnostic" => Verbosity.Diagnostic,
        _ => throw new ArgumentException($"Unknown --verbosity '{raw}'."),
    };

    private static IReadOnlyList<string>? SplitCsv(string? raw) =>
        raw is null ? null : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
