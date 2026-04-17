using System.CommandLine;

namespace XamlLint.Cli.Commands;

internal static class HookCommand
{
    public static Command Build()
    {
        var cmd = new Command("hook", "Read Claude Code hook JSON from stdin and run the lint pipeline.");

        cmd.SetAction((_, _) =>
        {
            System.Console.WriteLine("[stub] hook invoked. Wired to pipeline in Task 17.");
            return Task.FromResult(0);
        });

        return cmd;
    }
}
