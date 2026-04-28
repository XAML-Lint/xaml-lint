using System.CommandLine;
using XamlLint.Cli.Update;

namespace XamlLint.Cli.Commands;

internal static class UpdateCommand
{
    public static Command Build()
    {
        var checkOpt = new Option<bool>("--check")
        {
            Description = "Report whether an update is available without installing it.",
        };

        var cmd = new Command("update", "Update xaml-lint to the latest version on NuGet.");
        cmd.Options.Add(checkOpt);

        cmd.SetAction((parseResult, _) =>
        {
            var checkOnly = parseResult.GetValue(checkOpt);
            var exit = Handle(
                new UpdateOptions(CheckOnly: checkOnly),
                stdout: System.Console.Out,
                stderr: System.Console.Error,
                httpFactory: () => new HttpClient { Timeout = TimeSpan.FromSeconds(5) },
                processRunner: new DefaultProcessRunner());
            return Task.FromResult(exit);
        });

        return cmd;
    }

    public static int Handle(
        UpdateOptions options,
        TextWriter stdout,
        TextWriter stderr,
        Func<HttpClient> httpFactory,
        IProcessRunner processRunner)
    {
        // Filled in by Task 6.
        return 0;
    }
}

internal sealed record UpdateOptions(bool CheckOnly);
