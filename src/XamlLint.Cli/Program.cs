using System.CommandLine;
using System.CommandLine.Invocation;
using XamlLint.Cli.Commands;

namespace XamlLint.Cli;

internal static class Program
{
    public static Task<int> Main(string[] args)
        => BuildRoot().Parse(args).InvokeAsync();

    internal static RootCommand BuildRoot()
    {
        var root = new RootCommand("xaml-lint — XAML linter CLI.");
        root.Subcommands.Add(LintCommand.Build());
        root.Subcommands.Add(HookCommand.Build());
        root.Subcommands.Add(UpdateCommand.Build());

        OverrideVersionOption(root);

        return root;
    }

    private static void OverrideVersionOption(RootCommand root)
    {
        var versionOption = root.Options.OfType<VersionOption>().SingleOrDefault();
        if (versionOption is null) return;

        versionOption.Action = new AnonymousSynchronousCommandLineAction(
            parseResult =>
            {
                parseResult.InvocationConfiguration.Output.WriteLine(ToolVersion.Current);
                return 0;
            });
    }

    private sealed class AnonymousSynchronousCommandLineAction : SynchronousCommandLineAction
    {
        private readonly Func<ParseResult, int> _invoke;
        public AnonymousSynchronousCommandLineAction(Func<ParseResult, int> invoke) => _invoke = invoke;
        public override int Invoke(ParseResult parseResult) => _invoke(parseResult);
    }
}
