using System.Reflection;

namespace XamlLint.Cli;

public static class ToolVersion
{
    public static string Current { get; } = typeof(ToolVersion).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0-dev";
}
