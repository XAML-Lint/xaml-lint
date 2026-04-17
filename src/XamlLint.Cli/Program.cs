using System.CommandLine;
using XamlLint.Cli.Commands;

namespace XamlLint.Cli;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        var root = new RootCommand("xaml-lint — XAML linter CLI.");
        root.Subcommands.Add(LintCommand.Build());
        root.Subcommands.Add(HookCommand.Build());

        return root.Parse(args).InvokeAsync();
    }
}
