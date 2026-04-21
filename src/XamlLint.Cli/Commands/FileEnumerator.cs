using Microsoft.Extensions.FileSystemGlobbing;

namespace XamlLint.Cli.Commands;

/// <summary>
/// Expands positional path/glob arguments into concrete file paths, honoring <c>--include</c>
/// and <c>--exclude</c> filters. When <c>--force</c> is not set, files whose extension is
/// neither <c>.xaml</c> nor <c>.axaml</c> are returned with a flag so the caller can emit LX005.
/// </summary>
public static class FileEnumerator
{
    public sealed record EnumeratedFile(string AbsolutePath, bool IsXamlExtension);

    /// <summary>
    /// Returns true when the path ends with a XAML-family extension: <c>.xaml</c> (WPF / WinUI 3 /
    /// UWP / MAUI / Uno) or <c>.axaml</c> (Avalonia). Both are treated as first-class XAML files.
    /// </summary>
    public static bool IsXamlExtension(string path) =>
        path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<EnumeratedFile> Enumerate(
        IReadOnlyList<string> positional,
        IReadOnlyList<string>? stdinPaths,
        IReadOnlyList<string> include,
        IReadOnlyList<string> exclude,
        bool force,
        string workingDirectory)
    {
        var all = new List<string>();

        if (stdinPaths is not null)
        {
            all.AddRange(stdinPaths.Select(p => Path.GetFullPath(p, workingDirectory)));
        }

        foreach (var arg in positional)
        {
            if (arg == "-") continue; // handled as stdinPaths by caller
            if (File.Exists(arg))
            {
                all.Add(Path.GetFullPath(arg, workingDirectory));
                continue;
            }
            if (Directory.Exists(arg))
            {
                foreach (var pattern in new[] { "*.xaml", "*.axaml" })
                foreach (var f in Directory.EnumerateFiles(arg, pattern, SearchOption.AllDirectories))
                {
                    // Post-filter: on NTFS with short-name aliasing, "*.xaml" can return .axaml
                    // files (and vice versa). Distinct() below dedupes the double-add.
                    if (IsXamlExtension(f))
                        all.Add(Path.GetFullPath(f));
                }
                continue;
            }

            // Treat as glob relative to working directory.
            var matcher = new Matcher().AddInclude(arg);
            foreach (var hit in matcher.GetResultsInFullPath(workingDirectory))
                all.Add(hit);
        }

        if (include.Count > 0)
        {
            var inc = new Matcher();
            foreach (var pattern in include) inc.AddInclude(pattern);
            all = all.Where(f => inc.Match(Path.GetRelativePath(workingDirectory, f).Replace('\\', '/')).HasMatches).ToList();
        }

        if (exclude.Count > 0)
        {
            var exc = new Matcher();
            foreach (var pattern in exclude) exc.AddInclude(pattern);
            all = all.Where(f => !exc.Match(Path.GetRelativePath(workingDirectory, f).Replace('\\', '/')).HasMatches).ToList();
        }

        // Distinct + stable ordering.
        foreach (var path in all.Distinct().OrderBy(p => p, StringComparer.Ordinal))
        {
            var isXaml = IsXamlExtension(path);
            if (!isXaml && !force) yield return new EnumeratedFile(path, IsXamlExtension: false);
            else if (isXaml || force) yield return new EnumeratedFile(path, IsXamlExtension: isXaml);
        }
    }
}
