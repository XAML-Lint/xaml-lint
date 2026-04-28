using System.Reflection;

namespace XamlLint.Cli;

public static class ToolVersion
{
    public static string Current { get; } = StripBuildMetadata(
        typeof(ToolVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "0.0.0-dev");

    private static string StripBuildMetadata(string version)
    {
        var plusIndex = version.IndexOf('+');
        return plusIndex >= 0 ? version[..plusIndex] : version;
    }
}
