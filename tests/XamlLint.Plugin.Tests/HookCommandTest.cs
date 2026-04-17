using System.Text.Json;
using XamlLint.Cli.Commands;

namespace XamlLint.Plugin.Tests;

public sealed class HookCommandTest
{
    [Fact]
    public void Empty_payload_yields_empty_envelope_and_exit_zero()
    {
        using var tmp = new TempDir();
        using var stdin = new StringReader("{}");
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        var exit = HookCommand.Handle(stdin, stdout, stderr, tmp.Path);

        exit.Should().Be(0);
        var root = JsonDocument.Parse(stdout.ToString()).RootElement;
        root.GetProperty("results").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void Payload_with_existing_xaml_lints_the_file()
    {
        using var tmp = new TempDir();
        var file = Path.Combine(tmp.Path, "a.xaml");
        File.WriteAllText(file, "<Grid>"); // malformed

        var payload = new
        {
            tool_name = "Edit",
            tool_input = new { file_path = file }
        };
        var json = JsonSerializer.Serialize(payload);

        using var stdin = new StringReader(json);
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        var exit = HookCommand.Handle(stdin, stdout, stderr, tmp.Path);

        exit.Should().Be(1); // LX001 is error
        stdout.ToString().Should().Contain("LX001");
    }

    [Fact]
    public void Malformed_stdin_json_exits_two()
    {
        using var tmp = new TempDir();
        using var stdin = new StringReader("{ not json");
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        var exit = HookCommand.Handle(stdin, stdout, stderr, tmp.Path);

        exit.Should().Be(2);
        stderr.ToString().Should().Contain("malformed JSON");
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("xaml-lint-hook-").FullName;
        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { }
        }
    }
}
