using XamlLint.Cli;
using XamlLint.Cli.Commands;

namespace XamlLint.Cli.Tests;

public sealed class LintPipelineTest
{
    [Fact]
    public void Clean_xaml_emits_empty_results_and_exit_zero()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.xaml");
        File.WriteAllText(file, "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");

        var (exit, stdout) = Run(tmp, new[] { file });

        exit.Should().Be(0);
        stdout.Should().Contain("\"results\": []");
    }

    [Fact]
    public void Malformed_xaml_emits_LX001_and_exit_one()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "bad.xaml");
        File.WriteAllText(file, "<Grid>");

        var (exit, stdout) = Run(tmp, new[] { file });

        exit.Should().Be(1);
        stdout.Should().Contain("LX001");
    }

    [Fact]
    public void Non_xaml_without_force_emits_LX005_info()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.txt");
        File.WriteAllText(file, "not xaml");

        var (exit, stdout) = Run(tmp, new[] { file });

        exit.Should().Be(0); // info doesn't bump exit code
        stdout.Should().Contain("LX005");
    }

    [Fact]
    public void Unreadable_config_yields_exit_two()
    {
        using var tmp = new TempDir();
        File.WriteAllText(Path.Combine(tmp.Path, "xaml-lint.config.json"), "{ not valid");
        File.WriteAllText(Path.Combine(tmp.Path, "a.xaml"), "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");

        var (exit, _) = Run(tmp, new[] { Path.Combine(tmp.Path, "a.xaml") });

        exit.Should().Be(2);
    }

    private static (int Exit, string Stdout) Run(TempDir tmp, string[] args)
    {
        var opts = new LintOptions(
            Paths: args.ToList(),
            ReadFromStdin: false,
            Format: OutputFormat.CompactJson,
            OutputPath: null, ConfigPath: null, NoConfig: false,
            Dialect: null, OnlyRules: null,
            Include: Array.Empty<string>(), Exclude: Array.Empty<string>(),
            Verbosity: Verbosity.Normal, Force: false);

        using var stdout = new StringWriter();
        using var stdin = new StringReader("");
        var pipeline = new LintPipeline(stdout, stdin, tmp.Path);
        var exit = pipeline.Run(opts);
        return (exit, stdout.ToString());
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("xaml-lint-pipe-").FullName;
        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { }
        }
    }
}
