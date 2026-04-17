using XamlLint.DocTool;

namespace XamlLint.Core.Tests.DocTool;

public sealed class DocStubWriterTest
{
    [Fact]
    public void Run_creates_stubs_for_every_catalog_id()
    {
        using var tmp = new TempRepo();
        var actions = DocStubWriter.Run(tmp.Path, checkOnly: false);

        foreach (var id in GeneratedRuleCatalog.Rules.Select(r => r.Metadata.Id))
        {
            var path = Path.Combine(tmp.Path, "docs", "rules", $"{id}.md");
            File.Exists(path).Should().BeTrue($"docs/rules/{id}.md should exist after Run");
        }
    }

    [Fact]
    public void Check_only_mode_reports_missing_without_writing()
    {
        using var tmp = new TempRepo();
        var actions = DocStubWriter.Run(tmp.Path, checkOnly: true);

        actions.Where(a => a.Created).Should().NotBeEmpty();
        Directory.Exists(Path.Combine(tmp.Path, "docs", "rules")).Should().BeTrue();
        Directory.GetFiles(Path.Combine(tmp.Path, "docs", "rules")).Should().BeEmpty("check-only must not write");
    }

    [Fact]
    public void Orphan_stub_files_are_flagged_and_deleted()
    {
        using var tmp = new TempRepo();
        var docsDir = Path.Combine(tmp.Path, "docs", "rules");
        Directory.CreateDirectory(docsDir);
        var orphan = Path.Combine(docsDir, "LX998.md");
        File.WriteAllText(orphan, $"# LX998\n{DocStubWriter.StubSentinel} -->");

        DocStubWriter.Run(tmp.Path, checkOnly: false);

        File.Exists(orphan).Should().BeFalse();
    }

    private sealed class TempRepo : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("xaml-lint-doctool-").FullName;
        public TempRepo() { File.WriteAllText(System.IO.Path.Combine(Path, "xaml-lint.slnx"), "<Solution/>"); }
        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { }
        }
    }
}
