using System.Text;
using System.Text.RegularExpressions;

namespace XamlLint.Core.Tests.TestInfrastructure;

public sealed record ExpectedDiagnostic(
    string RuleId,
    int StartLine,
    int StartCol,
    int EndLine,
    int EndCol,
    bool HasSpan);

public sealed record ParsedTestCode(
    string Source,
    IReadOnlyList<ExpectedDiagnostic> Expected);

public static class XamlTestCode
{
    // Matches {|RULE:span|}, {|RULE|}, [|span|] in a single pass.
    private static readonly Regex MarkerPattern = new(
        @"\{\|(?<ruleFull>[A-Z]+\d+):|\{\|(?<ruleBare>[A-Z]+\d+)\|\}|\[\||\|\]|\|\}",
        RegexOptions.Compiled);

    public static ParsedTestCode Parse(string marked, string? defaultRuleId = null)
    {
        var sb = new StringBuilder(marked.Length);
        var expected = new List<ExpectedDiagnostic>();
        var activeStack = new Stack<ActiveMarker>();
        int line = 1, col = 1;
        int i = 0;

        while (i < marked.Length)
        {
            var m = MarkerPattern.Match(marked, i);
            if (!m.Success || m.Index != i)
            {
                var ch = marked[i];
                sb.Append(ch);
                if (ch == '\n') { line++; col = 1; }
                else { col++; }
                i++;
                continue;
            }

            if (m.Groups["ruleFull"].Success)
            {
                activeStack.Push(new ActiveMarker(m.Groups["ruleFull"].Value, line, col));
                i += m.Length;
            }
            else if (m.Groups["ruleBare"].Success)
            {
                expected.Add(new ExpectedDiagnostic(
                    RuleId: m.Groups["ruleBare"].Value,
                    StartLine: line, StartCol: col, EndLine: line, EndCol: col, HasSpan: false));
                i += m.Length;
            }
            else if (m.Value == "[|")
            {
                if (defaultRuleId is null)
                    throw new InvalidOperationException("Shorthand '[|...|]' requires a defaultRuleId.");
                activeStack.Push(new ActiveMarker(defaultRuleId, line, col));
                i += 2;
            }
            else if (m.Value == "|}" || m.Value == "|]")
            {
                if (activeStack.Count == 0)
                    throw new InvalidOperationException($"Unmatched '{m.Value}' at line {line}, col {col}.");
                var open = activeStack.Pop();
                expected.Add(new ExpectedDiagnostic(
                    RuleId: open.RuleId,
                    StartLine: open.Line, StartCol: open.Col,
                    EndLine: line, EndCol: col, HasSpan: true));
                i += 2;
            }
        }

        if (activeStack.Count > 0)
            throw new InvalidOperationException("Unclosed marker span.");

        return new ParsedTestCode(sb.ToString(), expected);
    }

    private sealed record ActiveMarker(string RuleId, int Line, int Col);
}
