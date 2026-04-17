using XamlLint.Core.Suppressions;

namespace XamlLint.Core.Tests.TestInfrastructure;

/// <summary>
/// Minimal harness: parse a marker-annotated string, run a single rule on it, assert the
/// emitted diagnostics match the marker set. Not a public surface; tests use this directly.
/// </summary>
public static class XamlDiagnosticVerifier<TRule> where TRule : IXamlRule, new()
{
    public static void Analyze(string markedSource, Dialect dialect = Dialect.Wpf)
    {
        var rule = new TRule();
        var parsed = XamlTestCode.Parse(markedSource, defaultRuleId: rule.Metadata.Id);
        var doc = XamlDocument.FromString(parsed.Source, "inline.xaml", dialect);

        doc.ParseError.Should().BeNull("the test source should be valid XAML");

        var pragma = PragmaParser.Parse(doc);
        var dispatcher = new RuleDispatcher(new IXamlRule[] { rule });
        var severities = new Dictionary<string, Severity> { [rule.Metadata.Id] = rule.Metadata.DefaultSeverity };
        var actual = dispatcher.Dispatch(doc, pragma.Map, severities);

        AssertDiagnosticsMatch(parsed.Expected, actual, rule.Metadata.Id);
    }

    private static void AssertDiagnosticsMatch(
        IReadOnlyList<ExpectedDiagnostic> expected,
        IReadOnlyList<Diagnostic> actual,
        string ruleId)
    {
        var relevant = actual.Where(a => a.RuleId == ruleId).ToList();
        relevant.Should().HaveCount(expected.Count,
            $"expected {expected.Count} {ruleId} diagnostic(s), got {relevant.Count}");

        for (var i = 0; i < expected.Count; i++)
        {
            var e = expected[i];
            if (!e.HasSpan) continue;

            relevant.Should().Contain(a =>
                a.StartLine == e.StartLine &&
                a.StartCol == e.StartCol &&
                a.EndLine == e.EndLine &&
                a.EndCol == e.EndCol,
                $"expected a {ruleId} diagnostic at ({e.StartLine},{e.StartCol})–({e.EndLine},{e.EndCol})");
        }
    }
}
