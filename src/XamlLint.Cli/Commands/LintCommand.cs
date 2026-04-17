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
        formatOpt.Validators.Add(r =>
        {
            var v = r.GetValueOrDefault<string?>();
            if (v is not null && v is not ("compact-json" or "sarif" or "msbuild" or "pretty"))
                r.AddError($"Unknown --format '{v}'. Allowed: compact-json, sarif, msbuild, pretty.");
        });
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
        verbosityOpt.Validators.Add(r =>
        {
            var v = r.GetValueOrDefault<string?>();
            if (v is null) return;
            var lower = v.ToLowerInvariant();
            if (lower is not ("q" or "quiet" or "m" or "minimal" or "n" or "normal" or "d" or "detailed" or "diag" or "diagnostic"))
                r.AddError($"Unknown --verbosity '{v}'. Allowed: quiet, minimal, normal, detailed, diagnostic (or q/m/n/d/diag).");
        });
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
            var opts = new LintOptions(
                Paths: parseResult.GetValue(pathsArg) ?? Array.Empty<string>(),
                ReadFromStdin: (parseResult.GetValue(pathsArg) ?? Array.Empty<string>()).Contains("-"),
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

            var pipeline = new LintPipeline(
                stdout: System.Console.Out,
                stderr: System.Console.Error,
                stdin: System.Console.In,
                workingDirectory: Environment.CurrentDirectory);
            return Task.FromResult(pipeline.Run(opts));
        });

        return cmd;
    }

    // Validators on formatOpt/verbosityOpt guarantee only legal values reach these mappers.
    private static OutputFormat? ParseFormat(string? raw) => raw switch
    {
        null => null,
        "compact-json" => OutputFormat.CompactJson,
        "sarif" => OutputFormat.Sarif,
        "msbuild" => OutputFormat.MsBuild,
        "pretty" => OutputFormat.Pretty,
        _ => null,
    };

    private static Verbosity ParseVerbosity(string? raw) => raw?.ToLowerInvariant() switch
    {
        "q" or "quiet" => Verbosity.Quiet,
        "m" or "minimal" => Verbosity.Minimal,
        "d" or "detailed" => Verbosity.Detailed,
        "diag" or "diagnostic" => Verbosity.Diagnostic,
        _ => Verbosity.Normal,
    };

    private static IReadOnlyList<string>? SplitCsv(string? raw) =>
        raw is null ? null : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
