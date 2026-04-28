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
        var current = ToolVersion.Current;
        stdout.WriteLine($"Current version: {current}");
        stdout.WriteLine("Checking for updates...");

        using var http = httpFactory();
        var probe = NuGetVersionProbe.GetLatestStableAsync(http).GetAwaiter().GetResult();

        if (probe.LatestVersion is null)
        {
            stderr.WriteLine($"Failed to check for updates: {probe.Error}");
            stderr.WriteLine("To update, run: dotnet tool update -g xaml-lint");
            return 1;
        }

        if (!IsNewerVersion(probe.LatestVersion, current))
        {
            stdout.WriteLine($"xaml-lint is up to date ({current}).");
            return 0;
        }

        stdout.WriteLine($"New version available: {current} → {probe.LatestVersion}");

        if (options.CheckOnly)
        {
            stdout.WriteLine("To update, run: dotnet tool update -g xaml-lint");
            return 0;
        }

        stdout.WriteLine("Running: dotnet tool update -g xaml-lint");

        int exitCode;
        try
        {
            exitCode = processRunner.Run("dotnet", new[] { "tool", "update", "-g", "xaml-lint" });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            stderr.WriteLine("dotnet is not on PATH. Install the .NET SDK and re-run xaml-lint update.");
            return 1;
        }

        if (exitCode != 0)
        {
            stderr.WriteLine("Update failed.");
            stderr.WriteLine("If xaml-lint is currently running, open a new shell and run:");
            stderr.WriteLine("  dotnet tool update -g xaml-lint");
            return 1;
        }

        stdout.WriteLine($"Successfully updated to {probe.LatestVersion}.");
        return 0;
    }

    // Returns true only when `candidate` is a strictly higher parseable version than `current`.
    // If either string fails to parse, returns false — i.e., we treat "can't tell" as "up to date"
    // rather than risking an unnecessary update against garbage data.
    private static bool IsNewerVersion(string candidate, string current)
    {
        if (Version.TryParse(candidate, out var c) && Version.TryParse(current, out var n))
            return c.CompareTo(n) > 0;
        return false;
    }
}

internal sealed record UpdateOptions(bool CheckOnly);
