using System.Text.Json.Serialization;

namespace XamlLint.Cli.Commands;

/// <summary>
/// Minimal shape of a Claude Code PostToolUse hook JSON payload. Only the field we actually
/// need is deserialized; unknown fields are silently ignored.
/// </summary>
public sealed record HookPayload(
    [property: JsonPropertyName("tool_name")] string? ToolName,
    [property: JsonPropertyName("tool_input")] HookToolInput? ToolInput);

public sealed record HookToolInput(
    [property: JsonPropertyName("file_path")] string? FilePath);
