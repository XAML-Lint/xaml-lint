using System.Reflection;
using System.Text.Json;

namespace XamlLint.Cli.Config;

/// <summary>
/// Loads bundled preset profiles (spec §5.4). Only three names resolve: <c>xaml-lint:off</c>,
/// <c>xaml-lint:recommended</c>, <c>xaml-lint:strict</c>.
/// </summary>
public static class PresetProfiles
{
    public static readonly IReadOnlySet<string> KnownNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "xaml-lint:off",
        "xaml-lint:recommended",
        "xaml-lint:strict",
    };

    public static ConfigDocument Load(string presetName)
    {
        if (!KnownNames.Contains(presetName))
            throw new ArgumentException($"Unknown preset '{presetName}'. Known: {string.Join(", ", KnownNames)}.");

        var fileName = presetName.Replace(':', '-'); // xaml-lint:recommended -> xaml-lint-recommended
        var resourceName = $"XamlLint.Cli.Presets.{fileName}.json";

        using var stream = typeof(PresetProfiles).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded preset '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<ConfigDocument>(json, ConfigJson.Options)
            ?? throw new InvalidOperationException($"Preset '{presetName}' deserialized to null.");
    }
}
