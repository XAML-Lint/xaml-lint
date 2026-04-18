using XamlLint.Core.Suppressions;

namespace XamlLint.Core;

/// <summary>
/// Per-file data passed into every rule invocation. Immutable.
/// </summary>
public sealed class RuleContext
{
    public required Dialect Dialect { get; init; }

    /// <summary>
    /// Effective severity per rule ID after preset + config + overrides resolution.
    /// A rule whose ID maps to <see cref="Severity.Info"/> still runs; a rule whose ID is
    /// missing from the map runs at its declared default (the dispatcher pre-populates the
    /// map so missing entries indicate a bug).
    /// </summary>
    public required IReadOnlyDictionary<string, Severity> SeverityMap { get; init; }

    public required SuppressionMap Suppressions { get; init; }

    /// <summary>
    /// Raw source characters (UTF-16 code units) of the linted file, for rules that need
    /// offsets beyond what the XDocument API exposes. Exposed as a read-only memory block
    /// so rules cannot mutate it.
    /// </summary>
    public required ReadOnlyMemory<char> Source { get; init; }

    /// <summary>
    /// Major version of the target framework (e.g. <c>10</c> for .NET 10), or <c>null</c>
    /// when the user hasn't opted into a specific framework. Rules use this together with
    /// <see cref="DialectFeatures"/> to gate features whose availability depends on the
    /// runtime platform — most prominently the WPF-on-.NET-10 grid definition shorthand.
    /// </summary>
    public int? FrameworkMajorVersion { get; init; }
}
