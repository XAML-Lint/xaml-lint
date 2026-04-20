using XamlLint.Core.NameResolution;
using XamlLint.Core.Suppressions;

namespace XamlLint.Core;

/// <summary>
/// Takes a parsed document and invokes every applicable rule. Tool rules (IToolRule) are
/// skipped — they register with the catalog but emit diagnostics from their pipeline sites.
/// Each rule runs in a try/catch; exceptions become LX006 diagnostics, never propagate.
/// </summary>
public sealed class RuleDispatcher
{
    public const string LX006 = "LX006";
    public const string HelpUriLX006 = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX006.md";

    private readonly IReadOnlyList<IXamlRule> _rules;

    public RuleDispatcher(IEnumerable<IXamlRule> rules)
    {
        _rules = rules.Where(r => r is not IToolRule).ToArray();
    }

    public IReadOnlyList<Diagnostic> Dispatch(
        XamlDocument document,
        SuppressionMap suppressions,
        IReadOnlyDictionary<string, Severity> severityMap,
        int? frameworkMajorVersion = null)
    {
        if (document.ParseError is not null) return Array.Empty<Diagnostic>();

        var context = new RuleContext
        {
            Dialect = document.Dialect,
            SeverityMap = severityMap,
            Suppressions = suppressions,
            Source = document.Source.AsMemory(),
            FrameworkMajorVersion = frameworkMajorVersion,
            NameIndexBuilder = document.Root is null
                ? null
                : () => XamlNameIndex.Build(document.Root),
        };

        var output = new List<Diagnostic>();

        foreach (var rule in _rules)
        {
            var meta = rule.Metadata;
            if ((meta.Dialects & document.Dialect) == 0) continue;
            if (!severityMap.TryGetValue(meta.Id, out var effective)) continue; // "off"

            IEnumerable<Diagnostic> raw;
            try
            {
                raw = rule.Analyze(document, context);
            }
            catch (Exception ex)
            {
                var lx006Severity = severityMap.TryGetValue(LX006, out var s) ? s : Severity.Error;
                output.Add(new Diagnostic(
                    RuleId: LX006,
                    Severity: lx006Severity,
                    Message: $"Rule '{meta.Id}' threw {ex.GetType().Name}: {ex.Message}",
                    File: document.FilePath,
                    StartLine: 1, StartCol: 1, EndLine: 1, EndCol: 1,
                    HelpUri: HelpUriLX006));
                continue;
            }

            foreach (var d in raw)
            {
                if (suppressions.IsSuppressed(d.RuleId, d.StartLine)) continue;
                output.Add(d with { Severity = effective });
            }
        }

        return output;
    }
}
