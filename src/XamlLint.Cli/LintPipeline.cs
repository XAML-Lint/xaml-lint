using Microsoft.Extensions.FileSystemGlobbing;
using XamlLint.Cli.Commands;
using XamlLint.Cli.Config;
using XamlLint.Cli.Formatters;
using XamlLint.Core;
using XamlLint.Core.Parsing;
using XamlLint.Core.Suppressions;

namespace XamlLint.Cli;

public sealed class LintPipeline
{
    private readonly TextWriter _stdout;
    private readonly TextReader _stdin;
    private readonly string _workingDirectory;

    public LintPipeline(TextWriter stdout, TextReader stdin, string workingDirectory)
    {
        _stdout = stdout;
        _stdin = stdin;
        _workingDirectory = workingDirectory;
    }

    public int Run(LintOptions opts)
    {
        var catalog = GeneratedRuleCatalog.Rules;
        var catalogIds = catalog.Select(r => r.Metadata.Id).ToList();

        // 1. Config
        var loader = new ConfigLoader();
        var loaded = opts.NoConfigLookup
            ? loader.LoadFallback(catalogIds, opts.Overrides.PresetOverride)
            : opts.ConfigPath is not null
                ? loader.Load(opts.ConfigPath, catalogIds, opts.Overrides.PresetOverride)
                : loader.Discover(_workingDirectory, catalogIds, opts.Overrides.PresetOverride);

        var allDiagnostics = new List<Diagnostic>(loaded.Diagnostics);
        var suppressedForSarif = new List<Diagnostic>();

        if (loaded.Config is null)
            return FinalizeAndExit(opts, allDiagnostics, suppressedForSarif, toolFailure: true);

        // Apply CLI rule overlay to the base severity map once, upstream of everything that
        // reads it — tool-level diagnostics (LX0001/LX0004/LX0005) and the per-file overlay
        // used by the dispatcher both see it.
        var config = opts.Overrides.RuleSeverities.Count > 0
            ? loaded.Config with { RuleSeverities = ApplyRuleOverlay(loaded.Config.RuleSeverities, opts.Overrides.RuleSeverities) }
            : loaded.Config;
        var severityMap = config.RuleSeverities;

        // 2. File enumeration (stdin only if "-" in positional)
        var stdinPaths = opts.ReadFromStdin
            ? ReadStdinPaths()
            : null;

        var files = FileEnumerator.Enumerate(
            positional: opts.Paths.Where(p => p != "-").ToList(),
            stdinPaths: stdinPaths,
            include: opts.Include,
            exclude: opts.Exclude,
            force: opts.Force,
            workingDirectory: _workingDirectory).ToList();

        // 3. Per-file processing
        var dispatcher = new RuleDispatcher(catalog);
        var forceDialect = ParseCliDialect(opts.Dialect);

        foreach (var ef in files)
        {
            if (!ef.IsXamlExtension)
            {
                if (severityMap.TryGetValue("LX0005", out var s5))
                    allDiagnostics.Add(new Diagnostic(
                        "LX0005", s5, $"Skipping non-XAML file '{ef.AbsolutePath}'.",
                        ef.AbsolutePath, 1, 1, 1, 1,
                        "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0005.md"));
                continue;
            }

            string source;
            try
            {
                source = File.ReadAllText(ef.AbsolutePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                if (severityMap.TryGetValue("LX0004", out var s4))
                    allDiagnostics.Add(new Diagnostic(
                        "LX0004", s4, $"Cannot read file '{ef.AbsolutePath}': {ex.Message}",
                        ef.AbsolutePath, 1, 1, 1, 1,
                        "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0004.md"));
                continue;
            }

            // Definitive root xmlns wins over CLI/config hints: a file whose root element
            // declares the MAUI or Avalonia namespace *is* that dialect's document regardless
            // of where it's stored or how the linter is invoked. --dialect and config entries
            // disambiguate only the shared WPF/WinUI/UWP presentation URL, for which Sniff
            // returns null. See docs/config-reference.md §"Dialect detection cascade".
            var dialect = DialectDetector.Sniff(source)
                ?? forceDialect
                ?? ResolveDialect(loaded.Config, ef.AbsolutePath, _workingDirectory)
                ?? DialectDetector.Fallback;

            var doc = XamlDocument.FromString(source, ef.AbsolutePath, dialect);

            if (doc.ParseError is not null)
            {
                if (severityMap.TryGetValue("LX0001", out var s1))
                    allDiagnostics.Add(new Diagnostic(
                        "LX0001", s1,
                        $"Malformed XAML: {doc.ParseError.Message}",
                        ef.AbsolutePath,
                        doc.ParseError.Line, doc.ParseError.Column,
                        doc.ParseError.Line, doc.ParseError.Column,
                        "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0001.md"));
                continue;
            }

            // ApplyOverridesForFile reads config.RuleSeverities (already CLI-overlaid above).
            var effective = ConfigLoader.ApplyOverridesForFile(config, ef.AbsolutePath, _workingDirectory);

            var pragma = opts.Overrides.NoInlineConfig
                ? new PragmaParser.PragmaParseResult(new SuppressionMap(), Array.Empty<Diagnostic>())
                : PragmaParser.Parse(doc);
            allDiagnostics.AddRange(pragma.Diagnostics);

            var frameworkMajorVersion = ResolveFrameworkMajorVersion(config, ef.AbsolutePath, _workingDirectory);
            var raw = dispatcher.Dispatch(doc, pragma.Map, effective, frameworkMajorVersion);

            foreach (var d in raw)
            {
                if (pragma.Map.IsSuppressed(d.RuleId, d.StartLine))
                    suppressedForSarif.Add(d);
                else
                    allDiagnostics.Add(d);
            }
        }

        return FinalizeAndExit(opts, allDiagnostics, suppressedForSarif, toolFailure: false);
    }

    private int FinalizeAndExit(
        LintOptions opts,
        List<Diagnostic> diagnostics,
        List<Diagnostic> suppressed,
        bool toolFailure)
    {
        // Verbosity filters.
        var filtered = opts.Verbosity switch
        {
            Verbosity.Quiet => diagnostics.Where(d => d.Severity == Severity.Error).ToList(),
            Verbosity.Minimal => diagnostics.Where(d => d.Severity != Severity.Info).ToList(),
            _ => diagnostics,
        };

        var format = opts.Format ?? (Console.IsOutputRedirected ? OutputFormat.CompactJson : OutputFormat.Pretty);

        TextWriter writer = _stdout;
        FileStream? outFile = null;
        if (opts.OutputPath is not null && opts.OutputPath != "-")
        {
            outFile = File.Create(opts.OutputPath);
            writer = new StreamWriter(outFile);
        }

        try
        {
            IDiagnosticFormatter formatter = format switch
            {
                OutputFormat.CompactJson => new CompactJsonFormatter(),
                OutputFormat.Sarif => new SarifFormatter(),
                OutputFormat.MsBuild => new MsBuildFormatter(),
                OutputFormat.Pretty => new PrettyFormatter(PrettyFormatter.ShouldUseColor()),
                _ => new CompactJsonFormatter(),
            };

            if (formatter is SarifFormatter sf)
                sf.Write(writer, filtered, ToolVersion.Current, suppressed);
            else
                formatter.Write(writer, filtered, ToolVersion.Current);
        }
        finally
        {
            if (outFile is not null) writer.Flush();
            outFile?.Dispose();
        }

        if (toolFailure) return 2;
        return filtered.Any(d => d.Severity == Severity.Error) ? 1 : 0;
    }

    private IReadOnlyList<string>? ReadStdinPaths()
    {
        var lines = new List<string>();
        string? line;
        while ((line = _stdin.ReadLine()) is not null)
        {
            line = line.Trim();
            if (line.Length > 0) lines.Add(line);
        }
        return lines;
    }

    private static IReadOnlyDictionary<string, Severity> ApplyRuleOverlay(
        IReadOnlyDictionary<string, Severity> baseMap,
        IReadOnlyDictionary<string, Severity?> overlay)
    {
        var combined = new Dictionary<string, Severity>(baseMap, StringComparer.Ordinal);
        foreach (var kv in overlay)
        {
            if (kv.Value is null) combined.Remove(kv.Key);
            else combined[kv.Key] = kv.Value.Value;
        }
        return combined;
    }

    private static Dialect? ParseCliDialect(string? raw) => raw?.ToLowerInvariant() switch
    {
        null => null,
        "wpf" => Dialect.Wpf,
        "winui3" => Dialect.WinUI3,
        "uwp" => Dialect.Uwp,
        "maui" => Dialect.Maui,
        "avalonia" => Dialect.Avalonia,
        "uno" => Dialect.Uno,
        _ => throw new ArgumentException($"Unknown --dialect '{raw}'."),
    };

    private static Dialect? ResolveDialect(ResolvedConfig cfg, string filePath, string baseDir)
    {
        foreach (var over in cfg.Overrides)
        {
            if (over.Dialect is null) continue;
            var matcher = new Microsoft.Extensions.FileSystemGlobbing.Matcher().AddInclude(over.FilesGlob);
            var rel = Path.GetRelativePath(baseDir, filePath).Replace('\\', '/');
            if (matcher.Match(rel).HasMatches) return over.Dialect;
        }
        return cfg.DefaultDialect;
    }

    private static int? ResolveFrameworkMajorVersion(ResolvedConfig cfg, string filePath, string baseDir)
    {
        foreach (var over in cfg.Overrides)
        {
            if (over.FrameworkMajorVersion is null) continue;
            var matcher = new Microsoft.Extensions.FileSystemGlobbing.Matcher().AddInclude(over.FilesGlob);
            var rel = Path.GetRelativePath(baseDir, filePath).Replace('\\', '/');
            if (matcher.Match(rel).HasMatches) return over.FrameworkMajorVersion;
        }
        return cfg.FrameworkMajorVersion;
    }
}
