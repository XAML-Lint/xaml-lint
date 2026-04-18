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
        var loaded = opts.NoConfig
            ? loader.LoadFallback(catalogIds)
            : opts.ConfigPath is not null
                ? loader.Load(opts.ConfigPath, catalogIds)
                : loader.Discover(_workingDirectory, catalogIds);

        var allDiagnostics = new List<Diagnostic>(loaded.Diagnostics);
        var suppressedForSarif = new List<Diagnostic>();

        if (loaded.Config is null)
            return FinalizeAndExit(opts, allDiagnostics, suppressedForSarif, toolFailure: true);

        var severityMap = loaded.Config.RuleSeverities;

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
                if (severityMap.TryGetValue("LX005", out var s5))
                    allDiagnostics.Add(new Diagnostic(
                        "LX005", s5, $"Skipping non-XAML file '{ef.AbsolutePath}'.",
                        ef.AbsolutePath, 1, 1, 1, 1,
                        "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX005.md"));
                continue;
            }

            string source;
            try
            {
                source = File.ReadAllText(ef.AbsolutePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                if (severityMap.TryGetValue("LX004", out var s4))
                    allDiagnostics.Add(new Diagnostic(
                        "LX004", s4, $"Cannot read file '{ef.AbsolutePath}': {ex.Message}",
                        ef.AbsolutePath, 1, 1, 1, 1,
                        "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX004.md"));
                continue;
            }

            var dialect = forceDialect
                ?? ResolveDialect(loaded.Config, ef.AbsolutePath, _workingDirectory)
                ?? DialectDetector.Sniff(source)
                ?? DialectDetector.Fallback;

            var doc = XamlDocument.FromString(source, ef.AbsolutePath, dialect);

            if (doc.ParseError is not null)
            {
                if (severityMap.TryGetValue("LX001", out var s1))
                    allDiagnostics.Add(new Diagnostic(
                        "LX001", s1,
                        $"Malformed XAML: {doc.ParseError.Message}",
                        ef.AbsolutePath,
                        doc.ParseError.Line, doc.ParseError.Column,
                        doc.ParseError.Line, doc.ParseError.Column,
                        "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX001.md"));
                continue;
            }

            var effective = ConfigLoader.ApplyOverridesForFile(loaded.Config, ef.AbsolutePath, _workingDirectory);
            if (opts.OnlyRules is { Count: > 0 })
                effective = effective.Where(kv => opts.OnlyRules.Contains(kv.Key)).ToDictionary(k => k.Key, v => v.Value);

            var pragma = PragmaParser.Parse(doc);
            allDiagnostics.AddRange(pragma.Diagnostics);

            var frameworkMajorVersion = ResolveFrameworkMajorVersion(loaded.Config, ef.AbsolutePath, _workingDirectory);
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
