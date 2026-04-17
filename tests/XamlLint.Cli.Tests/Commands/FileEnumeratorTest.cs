using XamlLint.Cli.Commands;

namespace XamlLint.Cli.Tests.Commands;

public sealed class FileEnumeratorTest
{
    [Fact]
    public void Direct_file_path_is_returned_when_xaml()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.xaml");
        File.WriteAllText(file, "<Root/>");

        var result = FileEnumerator.Enumerate(
            positional: new[] { file },
            stdinPaths: null,
            include: Array.Empty<string>(),
            exclude: Array.Empty<string>(),
            force: false,
            workingDirectory: tmp.Path).ToList();

        result.Should().ContainSingle(r => r.AbsolutePath == file && r.IsXamlExtension);
    }

    [Fact]
    public void Non_xaml_extension_flagged_unless_force()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.txt");
        File.WriteAllText(file, "not xaml");

        var withoutForce = FileEnumerator.Enumerate(new[] { file }, null, Array.Empty<string>(), Array.Empty<string>(), false, tmp.Path).Single();
        withoutForce.IsXamlExtension.Should().BeFalse();
    }

    [Fact]
    public void Exclude_removes_matching_files()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "a.xaml"), "<Root/>");
        File.WriteAllText(Path.Combine(tmp.Path, "b.xaml"), "<Root/>");

        var result = FileEnumerator.Enumerate(
            positional: new[] { tmp.Path },
            stdinPaths: null,
            include: Array.Empty<string>(),
            exclude: new[] { "b.xaml" },
            force: false,
            workingDirectory: tmp.Path).ToList();

        result.Should().ContainSingle(r => Path.GetFileName(r.AbsolutePath) == "a.xaml");
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("xaml-lint-enum-").FullName;
        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { }
        }
    }
}
