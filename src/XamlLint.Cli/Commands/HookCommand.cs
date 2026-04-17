using System.CommandLine;
using System.Text.Json;
using XamlLint.Cli.Config;

namespace XamlLint.Cli.Commands;

internal static class HookCommand
{
    public static Command Build()
    {
        var cmd = new Command("hook", "Read Claude Code hook JSON from stdin and run the lint pipeline.");

        cmd.SetAction((_, _) =>
        {
            var exitCode = Handle(
                stdin: System.Console.In,
                stdout: System.Console.Out,
                stderr: System.Console.Error,
                workingDirectory: Environment.CurrentDirectory);
            return Task.FromResult(exitCode);
        });

        return cmd;
    }

    public static int Handle(TextReader stdin, TextWriter stdout, TextWriter stderr, string workingDirectory)
    {
        var raw = stdin.ReadToEnd();
        HookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<HookPayload>(raw, ConfigJson.Options);
        }
        catch (JsonException ex)
        {
            stderr.WriteLine($"xaml-lint hook: malformed JSON on stdin: {ex.Message}");
            return 2;
        }

        var filePath = payload?.ToolInput?.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            // No file to lint; emit empty envelope so Claude sees a well-formed response.
            stdout.Write("""{"version":"1","tool":{"name":"xaml-lint","version":"dev"},"results":[]}""");
            return 0;
        }

        var opts = new LintOptions(
            Paths: new[] { filePath },
            ReadFromStdin: false,
            Format: OutputFormat.CompactJson,
            OutputPath: null, ConfigPath: null, NoConfig: false,
            Dialect: null, OnlyRules: null,
            Include: Array.Empty<string>(), Exclude: Array.Empty<string>(),
            Verbosity: Verbosity.Normal, Force: false);

        var pipeline = new LintPipeline(stdout, stderr, TextReader.Null, workingDirectory);
        return pipeline.Run(opts);
    }
}
