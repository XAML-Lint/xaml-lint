using XamlLint.Cli;
using XamlLint.Cli.Commands;
using XamlLint.Core;

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
    public void Clean_axaml_is_linted_not_skipped_with_LX005()
    {
        // Avalonia .axaml is a first-class XAML extension — the pipeline must
        // actually parse and analyse the file rather than emit LX005.
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.axaml");
        File.WriteAllText(file, "<UserControl xmlns=\"https://github.com/avaloniaui\" />");

        var (exit, stdout) = Run(tmp, new[] { file });

        exit.Should().Be(0);
        stdout.Should().NotContain("LX005");
        stdout.Should().Contain("\"results\": []");
    }

    [Fact]
    public void Malformed_axaml_emits_LX001_not_LX005()
    {
        // Real proof the file hits the parser: a broken .axaml surfaces LX001.
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "bad.axaml");
        File.WriteAllText(file, "<UserControl>");

        var (exit, stdout) = Run(tmp, new[] { file });

        exit.Should().Be(1);
        stdout.Should().Contain("LX001");
        stdout.Should().NotContain("LX005");
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

    [Fact]
    public void Cli_rule_override_turns_a_rule_on_when_preset_has_it_off()
    {
        // LX702 is DefaultEnabled=false → absent from :recommended. Force it on via --rule.
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.xaml");
        File.WriteAllText(file, """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <TextBox />
            </Grid>
            """);

        var (exit, stdout) = RunWith(tmp, new[] { file }, opts => opts with
        {
            Dialect = "wpf",
            Overrides = new CliOverrides(
                PresetOverride: null,
                RuleSeverities: new Dictionary<string, Severity?> { ["LX702"] = Severity.Warning },
                NoInlineConfig: false),
        });

        stdout.Should().Contain("LX702");
        exit.Should().Be(0); // warning doesn't bump exit code
    }

    [Fact]
    public void Cli_rule_override_turns_a_rule_off_when_preset_has_it_on()
    {
        // LX001 is enabled at error under :recommended; --rule LX001:off should silence it
        // even on a malformed file.
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "bad.xaml");
        File.WriteAllText(file, "<Grid>");

        var (exit, stdout) = RunWith(tmp, new[] { file }, opts => opts with
        {
            Overrides = new CliOverrides(
                PresetOverride: null,
                RuleSeverities: new Dictionary<string, Severity?> { ["LX001"] = null },
                NoInlineConfig: false),
        });

        stdout.Should().NotContain("LX001");
        exit.Should().Be(0);
    }

    [Fact]
    public void No_inline_config_ignores_disable_pragmas()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.xaml");
        File.WriteAllText(file, """
            <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <!-- xaml-lint disable LX100 -->
              <Button Grid.Row="5" />
            </Grid>
            """);

        var (exit1, stdout1) = RunWith(tmp, new[] { file }, opts => opts with { Dialect = "wpf" });
        var (exit2, stdout2) = RunWith(tmp, new[] { file }, opts => opts with
        {
            Dialect = "wpf",
            Overrides = CliOverrides.Empty with { NoInlineConfig = true },
        });

        // Pragma honoured by default → no LX100.
        stdout1.Should().NotContain("LX100");
        // With --no-inline-config → LX100 fires.
        stdout2.Should().Contain("LX100");
    }

    private static (int Exit, string Stdout) RunWith(TempDir tmp, string[] args, Func<LintOptions, LintOptions> customize)
    {
        var baseOpts = new LintOptions(
            Paths: args.ToList(),
            ReadFromStdin: false,
            Format: OutputFormat.CompactJson,
            OutputPath: null, ConfigPath: null, NoConfigLookup: false,
            Dialect: null, Overrides: CliOverrides.Empty,
            Include: Array.Empty<string>(), Exclude: Array.Empty<string>(),
            Verbosity: Verbosity.Normal, Force: false);

        using var stdout = new StringWriter();
        using var stdin = new StringReader("");
        var pipeline = new LintPipeline(stdout, stdin, tmp.Path);
        var exit = pipeline.Run(customize(baseOpts));
        return (exit, stdout.ToString());
    }

    private static (int Exit, string Stdout) Run(TempDir tmp, string[] args)
    {
        var opts = new LintOptions(
            Paths: args.ToList(),
            ReadFromStdin: false,
            Format: OutputFormat.CompactJson,
            OutputPath: null, ConfigPath: null, NoConfigLookup: false,
            Dialect: null, Overrides: CliOverrides.Empty,
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
