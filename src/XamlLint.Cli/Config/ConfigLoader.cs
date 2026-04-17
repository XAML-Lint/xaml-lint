using System.Text.Json;
using Microsoft.Extensions.FileSystemGlobbing;
using XamlLint.Core;

namespace XamlLint.Cli.Config;

/// <summary>
/// Discovers and resolves <c>xaml-lint.config.json</c>. Walks up from the given starting
/// directory looking for the file; stops at <c>.git</c> or filesystem root. Emits LX003
/// diagnostics for malformed configs (not exceptions — the CLI's exit code handler acts on
/// them).
/// </summary>
public sealed class ConfigLoader
{
    private const string ConfigFileName = "xaml-lint.config.json";
    private const string LX003 = "LX003";
    private const string HelpUriLX003 = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX003.md";

    public sealed record LoadResult(ResolvedConfig? Config, IReadOnlyList<Diagnostic> Diagnostics);

    public LoadResult Discover(string startDirectory, IReadOnlyList<string> catalogRuleIds)
    {
        var configPath = WalkUp(startDirectory);
        if (configPath is null)
            return LoadFallback(catalogRuleIds);

        return Load(configPath, catalogRuleIds);
    }

    public LoadResult Load(string configPath, IReadOnlyList<string> catalogRuleIds)
    {
        var diags = new List<Diagnostic>();
        ConfigDocument doc;
        try
        {
            var json = File.ReadAllText(configPath);
            doc = JsonSerializer.Deserialize<ConfigDocument>(json, ConfigJson.Options)
                ?? throw new JsonException("Config deserialized to null.");
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            diags.Add(Fail(configPath, $"Failed to read config '{configPath}': {ex.Message}"));
            return new LoadResult(null, diags);
        }

        var dialect = ParseDialect(doc.DefaultDialect, configPath, diags);
        if (dialect is null && diags.Count > 0)
            return new LoadResult(null, diags);

        var effective = ResolveSeverities(doc, catalogRuleIds, configPath, diags);
        var overrides = ResolveOverrides(doc.Overrides ?? Array.Empty<OverrideEntry>(), catalogRuleIds, configPath, diags);

        if (diags.Any(d => d.Severity == Severity.Error))
            return new LoadResult(null, diags);

        return new LoadResult(
            new ResolvedConfig(effective, dialect ?? Dialect.Wpf, overrides, configPath),
            diags);
    }

    public LoadResult LoadFallback(IReadOnlyList<string> catalogRuleIds)
    {
        // 1) Try the user-global config (spec §5.1).
        var userGlobal = UserGlobalConfigPath();
        if (userGlobal is not null && File.Exists(userGlobal))
            return Load(userGlobal, catalogRuleIds);

        // 2) Otherwise use the bundled "recommended" preset as final fallback.
        var recommended = PresetProfiles.Load("xaml-lint:recommended");
        var diags = new List<Diagnostic>();
        var severities = ResolveSeverities(recommended, catalogRuleIds, "xaml-lint:recommended", diags);
        return new LoadResult(new ResolvedConfig(severities, Dialect.Wpf, Array.Empty<ResolvedOverride>(), null), diags);
    }

    private static string? UserGlobalConfigPath()
    {
        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return string.IsNullOrEmpty(appData) ? null : Path.Combine(appData, "xaml-lint", "config.json");
        }

        var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrEmpty(xdg))
            return Path.Combine(xdg, "xaml-lint", "config.json");

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrEmpty(home) ? null : Path.Combine(home, ".config", "xaml-lint", "config.json");
    }

    private static string? WalkUp(string startDirectory)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, ConfigFileName);
            if (File.Exists(candidate)) return candidate;

            var gitDir = Path.Combine(dir.FullName, ".git");
            if (Directory.Exists(gitDir) || File.Exists(gitDir))
                return null;

            dir = dir.Parent;
        }
        return null;
    }

    private static Dialect? ParseDialect(string? raw, string sourcePath, List<Diagnostic> diags)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return raw.ToLowerInvariant() switch
        {
            "wpf" => Dialect.Wpf,
            "winui3" => Dialect.WinUI3,
            "uwp" => Dialect.Uwp,
            "maui" => Dialect.Maui,
            "avalonia" => Dialect.Avalonia,
            "uno" => Dialect.Uno,
            _ => Fail2()
        };

        Dialect? Fail2()
        {
            diags.Add(Fail(sourcePath, $"Unknown defaultDialect '{raw}'."));
            return null;
        }
    }

    private IReadOnlyDictionary<string, Severity> ResolveSeverities(
        ConfigDocument doc,
        IReadOnlyList<string> catalogIds,
        string sourcePath,
        List<Diagnostic> diags)
    {
        // Step 1: seed from extends preset (if any). Default extends = "xaml-lint:recommended".
        var presetName = doc.Extends ?? "xaml-lint:recommended";
        var effective = new Dictionary<string, Severity>();

        if (PresetProfiles.KnownNames.Contains(presetName))
        {
            var preset = PresetProfiles.Load(presetName);
            if (preset.Rules is not null)
                ApplyRuleBlock(preset.Rules, effective, catalogIds, sourcePath, diags, "preset");
        }
        else
        {
            diags.Add(Fail(sourcePath, $"'extends' must name a known preset (xaml-lint:off|recommended|strict); got '{presetName}'."));
        }

        // Step 2: overlay the user's rules block.
        if (doc.Rules is not null)
            ApplyRuleBlock(doc.Rules, effective, catalogIds, sourcePath, diags, "config");

        return effective;
    }

    private static void ApplyRuleBlock(
        IReadOnlyDictionary<string, JsonElement> rules,
        Dictionary<string, Severity> effective,
        IReadOnlyList<string> catalogIds,
        string sourcePath,
        List<Diagnostic> diags,
        string origin)
    {
        foreach (var kv in rules)
        {
            if (kv.Key != "*" && !catalogIds.Contains(kv.Key))
            {
                diags.Add(new Diagnostic(
                    RuleId: LX003, Severity: Severity.Warning,
                    Message: $"Unknown rule ID '{kv.Key}' in {origin} '{sourcePath}'.",
                    File: sourcePath, StartLine: 1, StartCol: 1, EndLine: 1, EndCol: 1,
                    HelpUri: HelpUriLX003));
                continue;
            }

            if (!TryParseSeverity(kv.Value, out var sev, out var reason))
            {
                diags.Add(Fail(sourcePath, $"Rule '{kv.Key}': {reason}"));
                continue;
            }

            if (kv.Key == "*")
            {
                foreach (var id in catalogIds)
                {
                    if (sev is null) effective.Remove(id);
                    else effective[id] = sev.Value;
                }
                continue;
            }

            if (sev is null) effective.Remove(kv.Key);
            else effective[kv.Key] = sev.Value;
        }
    }

    private static bool TryParseSeverity(JsonElement el, out Severity? sev, out string reason)
    {
        reason = string.Empty;
        sev = null;

        if (el.ValueKind == JsonValueKind.String)
            return TryParseSeverityString(el.GetString()!, out sev, out reason);

        if (el.ValueKind == JsonValueKind.Object)
        {
            if (!el.TryGetProperty("severity", out var sevEl))
            {
                reason = "object form requires a 'severity' property.";
                return false;
            }
            return TryParseSeverityString(sevEl.GetString() ?? "", out sev, out reason);
        }

        reason = $"unexpected value kind {el.ValueKind}.";
        return false;
    }

    private static bool TryParseSeverityString(string raw, out Severity? sev, out string reason)
    {
        reason = string.Empty;
        switch (raw.ToLowerInvariant())
        {
            case "off":     sev = null; return true;
            case "info":    sev = Severity.Info; return true;
            case "warning": sev = Severity.Warning; return true;
            case "error":   sev = Severity.Error; return true;
            default:
                sev = null;
                reason = $"unknown severity '{raw}' (expected off|info|warning|error).";
                return false;
        }
    }

    private static IReadOnlyList<ResolvedOverride> ResolveOverrides(
        IReadOnlyList<OverrideEntry> raw,
        IReadOnlyList<string> catalogIds,
        string sourcePath,
        List<Diagnostic> diags)
    {
        var result = new List<ResolvedOverride>();
        foreach (var o in raw)
        {
            var dialect = o.Dialect is null ? (Dialect?)null : ParseDialect(o.Dialect, sourcePath, diags);
            var ruleSeverities = new Dictionary<string, Severity?>();
            if (o.Rules is not null)
                CollectOverrideRuleBlock(o.Rules, ruleSeverities, catalogIds, sourcePath, diags);
            result.Add(new ResolvedOverride(o.Files, dialect, ruleSeverities));
        }
        return result;
    }

    // Override rule blocks preserve the "off" sentinel (null) so that per-file application
    // can remove rules from the combined map.
    private static void CollectOverrideRuleBlock(
        IReadOnlyDictionary<string, JsonElement> rules,
        Dictionary<string, Severity?> collected,
        IReadOnlyList<string> catalogIds,
        string sourcePath,
        List<Diagnostic> diags)
    {
        foreach (var kv in rules)
        {
            if (kv.Key != "*" && !catalogIds.Contains(kv.Key))
            {
                diags.Add(new Diagnostic(
                    RuleId: LX003, Severity: Severity.Warning,
                    Message: $"Unknown rule ID '{kv.Key}' in override '{sourcePath}'.",
                    File: sourcePath, StartLine: 1, StartCol: 1, EndLine: 1, EndCol: 1,
                    HelpUri: HelpUriLX003));
                continue;
            }

            if (!TryParseSeverity(kv.Value, out var sev, out var reason))
            {
                diags.Add(Fail(sourcePath, $"Rule '{kv.Key}': {reason}"));
                continue;
            }

            if (kv.Key == "*")
            {
                foreach (var id in catalogIds)
                    collected[id] = sev;
                continue;
            }

            collected[kv.Key] = sev;
        }
    }

    /// <summary>
    /// Applies per-file overrides (first-match-wins, spec §5.2) to produce the effective
    /// rule-severity map for a given file path.
    /// </summary>
    public static IReadOnlyDictionary<string, Severity> ApplyOverridesForFile(
        ResolvedConfig config,
        string filePath,
        string baseDir)
    {
        foreach (var over in config.Overrides)
        {
            var matcher = new Matcher().AddInclude(over.FilesGlob);
            var rel = Path.GetRelativePath(baseDir, filePath).Replace('\\', '/');
            if (matcher.Match(rel).HasMatches)
            {
                var combined = new Dictionary<string, Severity>(config.RuleSeverities);
                foreach (var kv in over.RuleSeverities)
                {
                    if (kv.Value is null) combined.Remove(kv.Key);
                    else combined[kv.Key] = kv.Value.Value;
                }
                return combined;
            }
        }
        return config.RuleSeverities;
    }

    private static Diagnostic Fail(string file, string msg) => new(
        RuleId: LX003, Severity: Severity.Error,
        Message: msg, File: file, StartLine: 1, StartCol: 1, EndLine: 1, EndCol: 1,
        HelpUri: HelpUriLX003);
}
