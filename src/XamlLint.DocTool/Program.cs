namespace XamlLint.DocTool;

internal static class Program
{
    public static int Main(string[] args)
    {
        var checkOnly = args.Contains("--check");

        string repoRoot;
        try { repoRoot = RepoPath.FindRoot(); }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex.Message);
            return 2;
        }

        var stubs    = DocStubWriter.Run(repoRoot, checkOnly);
        var schema   = SchemaWriter.Run(repoRoot, checkOnly);
        var presets  = PresetWriter.Run(repoRoot, checkOnly);

        var drift =
            stubs.Any(s => s.Created || s.Deleted) ||
            schema.Changed ||
            presets.Any(p => p.Changed);

        if (!checkOnly)
        {
            foreach (var s in stubs.Where(x => x.Created)) System.Console.WriteLine($"Created stub: {s.Path}");
            foreach (var s in stubs.Where(x => x.Deleted)) System.Console.WriteLine($"Deleted orphan stub: {s.Path}");
            if (schema.Changed) System.Console.WriteLine($"Updated schema: {schema.Path}");
            foreach (var p in presets.Where(x => x.Changed)) System.Console.WriteLine($"Updated preset: {p.Path}");
            return 0;
        }

        if (drift)
        {
            System.Console.Error.WriteLine("DocTool --check failed: the following outputs are stale.");
            foreach (var s in stubs.Where(x => x.Created)) System.Console.Error.WriteLine($"  missing stub: {s.Path}");
            foreach (var s in stubs.Where(x => x.Deleted)) System.Console.Error.WriteLine($"  orphan stub : {s.Path}");
            if (schema.Changed) System.Console.Error.WriteLine($"  stale schema: {schema.Path}");
            foreach (var p in presets.Where(x => x.Changed)) System.Console.Error.WriteLine($"  stale preset: {p.Path}");
            return 1;
        }

        System.Console.WriteLine("DocTool --check: no drift.");
        return 0;
    }
}
