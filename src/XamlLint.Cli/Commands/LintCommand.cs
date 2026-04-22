using System.CommandLine;
using XamlLint.Core;

namespace XamlLint.Cli.Commands;

internal static class LintCommand
{
    // CLI preset names drop the "xaml-lint:" prefix; this table is the bridge to the
    // internal preset identifiers used by PresetProfiles / ConfigLoader.
    private static readonly IReadOnlyDictionary<string, string> CliToInternalPreset =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["recommended"] = "xaml-lint:recommended",
            ["strict"]      = "xaml-lint:strict",
            ["none"]        = "xaml-lint:off",
        };

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
        var configOpt = new Option<string?>("--config", "-c")
        {
            Description = "Explicit config file; disables discovery.",
        };
        var noConfigLookupOpt = new Option<bool>("--no-config-lookup")
        {
            Description = "Skip config discovery entirely; use built-in defaults.",
        };
        var dialectOpt = new Option<string?>("--dialect")
        {
            Description = "Force dialect; overrides config.",
        };

        var ruleOpt = new Option<string[]>("--rule")
        {
            Description = "Override a rule's severity. Short form 'ID:severity' (off|info|warning|error), " +
                          "comma-separable and repeatable; or object form '{\"ID\":\"severity\",...}'.",
            AllowMultipleArgumentsPerToken = false,
        };
        var presetOpt = new Option<string?>("--preset")
        {
            Description = "Preset: recommended | strict | none. Overrides any 'extends' in config.",
        };
        presetOpt.Validators.Add(r =>
        {
            var v = r.GetValueOrDefault<string?>();
            if (v is not null && !CliToInternalPreset.ContainsKey(v))
                r.AddError($"Unknown --preset '{v}'. Allowed: recommended, strict, none.");
        });
        var noInlineConfigOpt = new Option<bool>("--no-inline-config")
        {
            Description = "Ignore <!-- xaml-lint ... --> pragmas inside files.",
        };
        var onlyOpt = new Option<string[]>("--only")
        {
            Description = "Shortcut: run only the listed rules. Equivalent to '--preset none --no-config-lookup --rule ID:sev...'. " +
                          "Severity defaults to the rule's DefaultSeverity. Mutually exclusive with --preset/--rule/--config/--no-config-lookup.",
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
        cmd.Options.Add(noConfigLookupOpt);
        cmd.Options.Add(dialectOpt);
        cmd.Options.Add(ruleOpt);
        cmd.Options.Add(presetOpt);
        cmd.Options.Add(noInlineConfigOpt);
        cmd.Options.Add(onlyOpt);
        cmd.Options.Add(includeOpt);
        cmd.Options.Add(excludeOpt);
        cmd.Options.Add(verbosityOpt);
        cmd.Options.Add(forceOpt);

        cmd.Validators.Add(r =>
        {
            var onlyVals = r.GetValue(onlyOpt);
            if (onlyVals is null || onlyVals.Length == 0) return;
            if (r.GetResult(presetOpt) is not null) r.AddError("--only is mutually exclusive with --preset.");
            if ((r.GetValue(ruleOpt) ?? Array.Empty<string>()).Length > 0)
                r.AddError("--only is mutually exclusive with --rule.");
            if (r.GetResult(configOpt) is not null) r.AddError("--only is mutually exclusive with --config.");
            if (r.GetValue(noConfigLookupOpt)) r.AddError("--only is mutually exclusive with --no-config-lookup.");
        });

        cmd.SetAction((parseResult, _) =>
        {
            var paths = parseResult.GetValue(pathsArg) ?? Array.Empty<string>();
            var catalog = GeneratedRuleCatalog.Rules
                .ToDictionary(rule => rule.Metadata.Id, rule => rule.Metadata.DefaultSeverity, StringComparer.Ordinal);

            var onlyVals = parseResult.GetValue(onlyOpt) ?? Array.Empty<string>();
            var ruleVals = parseResult.GetValue(ruleOpt) ?? Array.Empty<string>();

            string? presetOverride = null;
            bool forceNoConfigLookup = false;
            IReadOnlyDictionary<string, Severity?> ruleSev = new Dictionary<string, Severity?>(0);

            if (onlyVals.Length > 0)
            {
                var exp = OnlyExpansion.Expand(onlyVals, catalog);
                foreach (var err in exp.Errors) parseResult.InvocationConfiguration.Error.WriteLine(err);
                if (exp.Errors.Count > 0) return Task.FromResult(2);

                presetOverride = exp.PresetOverride;
                forceNoConfigLookup = exp.ForceNoConfigLookup;
                ruleSev = exp.Severities;
            }
            else
            {
                if (parseResult.GetValue(presetOpt) is { } presetCli)
                    presetOverride = CliToInternalPreset[presetCli];

                if (ruleVals.Length > 0)
                {
                    var parsed = RuleOverrideParser.Parse(ruleVals, catalog.Keys.ToList());
                    foreach (var err in parsed.Errors) parseResult.InvocationConfiguration.Error.WriteLine(err);
                    if (parsed.Errors.Count > 0) return Task.FromResult(2);
                    ruleSev = parsed.Severities;
                }
            }

            var overrides = new CliOverrides(
                PresetOverride: presetOverride,
                RuleSeverities: ruleSev,
                NoInlineConfig: parseResult.GetValue(noInlineConfigOpt));

            var opts = new LintOptions(
                Paths: paths,
                ReadFromStdin: paths.Contains("-"),
                Format: ParseFormat(parseResult.GetValue(formatOpt)),
                OutputPath: parseResult.GetValue(outputOpt),
                ConfigPath: parseResult.GetValue(configOpt),
                NoConfigLookup: parseResult.GetValue(noConfigLookupOpt) || forceNoConfigLookup,
                Dialect: parseResult.GetValue(dialectOpt),
                Overrides: overrides,
                Include: parseResult.GetValue(includeOpt) ?? Array.Empty<string>(),
                Exclude: parseResult.GetValue(excludeOpt) ?? Array.Empty<string>(),
                Verbosity: ParseVerbosity(parseResult.GetValue(verbosityOpt)),
                Force: parseResult.GetValue(forceOpt));

            var pipeline = new LintPipeline(
                stdout: System.Console.Out,
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
}
