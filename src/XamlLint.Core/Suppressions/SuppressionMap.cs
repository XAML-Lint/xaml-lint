namespace XamlLint.Core.Suppressions;

/// <summary>
/// Per-file suppression ranges keyed by rule ID. Task 4's <c>PragmaParser</c> populates
/// this via the internal <see cref="AddRange"/> method; callers only query with
/// <see cref="IsSuppressed"/>. Use "*" as the key for <c>disable All</c>.
/// </summary>
public sealed class SuppressionMap
{
    private readonly Dictionary<string, List<(int Start, int End)>> _ranges = new();

    public bool IsEmpty => _ranges.Count == 0;

    internal void AddRange(string ruleId, int startLine, int endLine)
    {
        if (endLine < startLine) return;
        if (!_ranges.TryGetValue(ruleId, out var list))
        {
            list = new List<(int, int)>();
            _ranges[ruleId] = list;
        }
        list.Add((startLine, endLine));
    }

    public bool IsSuppressed(string ruleId, int line)
    {
        if (_ranges.TryGetValue(ruleId, out var list) && list.Any(r => line >= r.Start && line <= r.End))
            return true;
        if (_ranges.TryGetValue("*", out var any) && any.Any(r => line >= r.Start && line <= r.End))
            return true;
        return false;
    }
}
