using System.Text.Json;
using System.Text.Json.Serialization;

namespace XamlLint.Cli.Config;

/// <summary>
/// Strongly-typed view of <c>xaml-lint.config.json</c>. Matches spec §5.2.
/// </summary>
public sealed record ConfigDocument(
    [property: JsonPropertyName("$schema")] string? Schema,
    [property: JsonPropertyName("extends")] string? Extends,
    [property: JsonPropertyName("defaultDialect")] string? DefaultDialect,
    [property: JsonPropertyName("frameworkVersion")] string? FrameworkVersion,
    [property: JsonPropertyName("overrides")] IReadOnlyList<OverrideEntry>? Overrides,
    [property: JsonPropertyName("rules")] IReadOnlyDictionary<string, JsonElement>? Rules);

public sealed record OverrideEntry(
    [property: JsonPropertyName("files")] string Files,
    [property: JsonPropertyName("dialect")] string? Dialect,
    [property: JsonPropertyName("frameworkVersion")] string? FrameworkVersion,
    [property: JsonPropertyName("rules")] IReadOnlyDictionary<string, JsonElement>? Rules);

public static class ConfigJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };
}
