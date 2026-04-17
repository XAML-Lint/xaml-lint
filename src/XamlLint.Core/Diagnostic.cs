namespace XamlLint.Core;

/// <summary>
/// A single finding emitted by the engine. Spans are 1-based, inclusive, and use the raw
/// source text's character columns (not UTF-16 code units).
/// </summary>
public sealed record Diagnostic(
    string RuleId,
    Severity Severity,
    string Message,
    string File,
    int StartLine,
    int StartCol,
    int EndLine,
    int EndCol,
    string? HelpUri);
