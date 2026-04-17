namespace XamlLint.DocTool;

internal static class RepoPath
{
    public static string FindRoot()
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) return dir.FullName;
            if (File.Exists(Path.Combine(dir.FullName, "xaml-lint.slnx"))) return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not find repo root (no .git or xaml-lint.slnx found upward).");
    }
}
