using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace XamlLint.Core.Suppressions;

/// <summary>
/// Parses <c>&lt;!-- xaml-lint ... --&gt;</c> directives into a <see cref="SuppressionMap"/>.
/// Emits LX002 for malformed directives whose body starts with the <c>xaml-lint</c> token.
/// Grammar: spec §3.4.
/// </summary>
public static class PragmaParser
{
    private const string LX002 = "LX002";
    private const string HelpUriLX002 = "https://github.com/jizc/xaml-lint/blob/main/docs/rules/LX002.md";

    private static readonly Regex RuleIdPattern = new(@"^[A-Z]+\d+$", RegexOptions.Compiled);

    public sealed record PragmaParseResult(SuppressionMap Map, IReadOnlyList<Diagnostic> Diagnostics);

    public static PragmaParseResult Parse(XamlDocument document)
    {
        var map = new SuppressionMap();
        var diags = new List<Diagnostic>();

        if (document.Document is null)
        {
            return new PragmaParseResult(map, diags);
        }

        var totalLines = CountLines(document.Source);
        var comments = document.Document.DescendantNodes().OfType<XComment>().ToList();
        var elementsInOrder = document.Document.Descendants()
            .Cast<XObject>()
            .Where(o => ((IXmlLineInfo)o).HasLineInfo())
            .OrderBy(o => ((IXmlLineInfo)o).LineNumber)
            .ThenBy(o => ((IXmlLineInfo)o).LinePosition)
            .ToList();

        // Active "disable" ranges awaiting matching "restore": ruleId -> startLine
        var active = new Dictionary<string, int>();

        foreach (var comment in comments)
        {
            var body = comment.Value.Trim();
            if (!body.StartsWith("xaml-lint", StringComparison.Ordinal)) continue;

            var info = (IXmlLineInfo)comment;
            var line = info.HasLineInfo() ? info.LineNumber : 1;
            var col = info.HasLineInfo() ? info.LinePosition : 1;

            if (!TryParseDirective(body, out var directive, out var isOnce, out var targets))
            {
                diags.Add(new Diagnostic(
                    LX002, Severity.Warning,
                    Message: $"Malformed xaml-lint pragma: '{body}'.",
                    File: document.FilePath,
                    StartLine: line, StartCol: col, EndLine: line, EndCol: col + body.Length + 7,
                    HelpUri: HelpUriLX002));
                continue;
            }

            switch (directive)
            {
                case "disable" when isOnce:
                {
                    var nextElement = FindNextElementAfter(elementsInOrder, line, col);
                    if (nextElement is null) break;
                    var nextInfo = (IXmlLineInfo)nextElement;
                    var elementEnd = ComputeElementEndLine(nextElement);
                    foreach (var t in targets)
                        map.AddRange(t, nextInfo.LineNumber, elementEnd);
                    break;
                }
                case "disable":
                {
                    foreach (var t in targets)
                    {
                        if (!active.ContainsKey(t))
                            active[t] = line;
                    }
                    break;
                }
                case "restore":
                {
                    foreach (var t in targets)
                    {
                        if (active.TryGetValue(t, out var start))
                        {
                            map.AddRange(t, start, line);
                            active.Remove(t);
                        }
                    }
                    break;
                }
            }
        }

        // Any still-open disable extends to end of file.
        foreach (var (ruleId, start) in active)
        {
            map.AddRange(ruleId, start, totalLines);
        }

        return new PragmaParseResult(map, diags);
    }

    private static bool TryParseDirective(string body, out string directive, out bool isOnce, out string[] targets)
    {
        directive = string.Empty;
        isOnce = false;
        targets = Array.Empty<string>();

        var parts = body.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts[0] != "xaml-lint") return false;

        var idx = 1;
        directive = parts[idx++];
        if (directive != "disable" && directive != "restore") return false;

        if (directive == "disable" && idx < parts.Length && parts[idx] == "once")
        {
            isOnce = true;
            idx++;
        }

        var rest = parts[idx..];
        if (rest.Length == 0)
        {
            if (directive == "restore") return false;
            targets = new[] { "*" };
            return true;
        }

        if (rest.Length == 1 && rest[0] == "All")
        {
            if (isOnce) return false; // "disable once All" is not a valid combo
            targets = new[] { "*" };
            return true;
        }

        foreach (var r in rest)
        {
            if (!RuleIdPattern.IsMatch(r)) return false;
        }
        targets = rest;
        return true;
    }

    private static XElement? FindNextElementAfter(List<XObject> ordered, int line, int col)
    {
        foreach (var o in ordered)
        {
            if (o is not XElement el) continue;
            var info = (IXmlLineInfo)el;
            if (info.LineNumber > line || (info.LineNumber == line && info.LinePosition > col))
                return el;
        }
        return null;
    }

    private static int ComputeElementEndLine(XElement el)
    {
        // Rough approximation: an element's "end" is the last line containing any of its
        // descendants or text. XDocument doesn't expose end-line natively, so we walk.
        var startInfo = (IXmlLineInfo)el;
        var maxLine = startInfo.LineNumber;
        foreach (var n in el.DescendantNodesAndSelf())
        {
            if (n is IXmlLineInfo li && li.HasLineInfo() && li.LineNumber > maxLine)
                maxLine = li.LineNumber;
        }
        return maxLine;
    }

    private static int CountLines(string source)
    {
        if (source.Length == 0) return 1;
        var count = 1;
        foreach (var ch in source)
        {
            if (ch == '\n') count++;
        }
        return count;
    }
}
