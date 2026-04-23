using XamlLint.Core.Suppressions;

namespace XamlLint.Core.Tests;

public sealed class RuleDispatcherTest
{
    private static XamlDocument WpfDoc(string src) => XamlDocument.FromString(src, "f.xaml", Dialect.Wpf);

    [Fact]
    public void Rule_not_matching_dialect_is_skipped()
    {
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var rule = new FakeRule("LX0100", Dialect.Maui, _ => new[] { Sample("LX0100", 1) });
        var dispatcher = new RuleDispatcher(new IXamlRule[] { rule });

        var diags = dispatcher.Dispatch(doc, new SuppressionMap(), BuildSeverityMap(rule));

        diags.Should().BeEmpty();
    }

    [Fact]
    public void Matching_dialect_runs_rule_and_emits_diagnostic()
    {
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var rule = new FakeRule("LX0100", Dialect.All, _ => new[] { Sample("LX0100", 1) });
        var dispatcher = new RuleDispatcher(new IXamlRule[] { rule });

        var diags = dispatcher.Dispatch(doc, new SuppressionMap(), BuildSeverityMap(rule));

        diags.Should().ContainSingle(d => d.RuleId == "LX0100");
    }

    [Fact]
    public void Effective_severity_overrides_default()
    {
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var rule = new FakeRule("LX0100", Dialect.All, _ => new[] { Sample("LX0100", 1, Severity.Warning) });
        var dispatcher = new RuleDispatcher(new IXamlRule[] { rule });

        var severities = new Dictionary<string, Severity> { ["LX0100"] = Severity.Error };
        var diags = dispatcher.Dispatch(doc, new SuppressionMap(), severities);

        diags.Single().Severity.Should().Be(Severity.Error);
    }

    [Fact]
    public void Rule_with_severity_off_does_not_run()
    {
        var ran = false;
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var rule = new FakeRule("LX0100", Dialect.All, _ => { ran = true; return new[] { Sample("LX0100", 1) }; });
        var dispatcher = new RuleDispatcher(new IXamlRule[] { rule });

        var severities = new Dictionary<string, Severity>(); // empty => "off"
        var diags = dispatcher.Dispatch(doc, new SuppressionMap(), severities);

        ran.Should().BeFalse();
        diags.Should().BeEmpty();
    }

    [Fact]
    public void Suppressed_diagnostics_are_dropped()
    {
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var rule = new FakeRule("LX0100", Dialect.All, _ => new[] { Sample("LX0100", 5) });
        var map = new SuppressionMap();
        typeof(SuppressionMap)
            .GetMethod("AddRange", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(map, new object[] { "LX0100", 1, 10 });

        var dispatcher = new RuleDispatcher(new IXamlRule[] { rule });
        var diags = dispatcher.Dispatch(doc, map, BuildSeverityMap(rule));

        diags.Should().BeEmpty();
    }

    [Fact]
    public void Throwing_rule_yields_LX0006_and_continues()
    {
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var bad = new FakeRule("LX0100", Dialect.All, _ => throw new InvalidOperationException("boom"));
        var good = new FakeRule("LX0101", Dialect.All, _ => new[] { Sample("LX0101", 1) });
        var dispatcher = new RuleDispatcher(new IXamlRule[] { bad, good });
        var severities = new Dictionary<string, Severity>
        {
            ["LX0100"] = Severity.Warning,
            ["LX0101"] = Severity.Warning,
            ["LX0006"] = Severity.Error,
        };

        var diags = dispatcher.Dispatch(doc, new SuppressionMap(), severities);

        diags.Should().Contain(d => d.RuleId == "LX0006" && d.Message.Contains("LX0100"));
        diags.Should().Contain(d => d.RuleId == "LX0101");
    }

    [Fact]
    public void Tool_rules_are_skipped()
    {
        var doc = WpfDoc("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var ran = false;
        var toolRule = new FakeToolRule("LX0001", _ => { ran = true; return Array.Empty<Diagnostic>(); });
        var dispatcher = new RuleDispatcher(new IXamlRule[] { toolRule });
        var severities = new Dictionary<string, Severity> { ["LX0001"] = Severity.Error };

        dispatcher.Dispatch(doc, new SuppressionMap(), severities);

        ran.Should().BeFalse();
    }

    private static Diagnostic Sample(string ruleId, int line, Severity sev = Severity.Warning) =>
        new(ruleId, sev, "msg", "f.xaml", line, 1, line, 10, null);

    private static IReadOnlyDictionary<string, Severity> BuildSeverityMap(IXamlRule rule) =>
        new Dictionary<string, Severity> { [rule.Metadata.Id] = rule.Metadata.DefaultSeverity };

    private sealed class FakeRule(string id, Dialect dialects, Func<XamlDocument, IEnumerable<Diagnostic>> body) : IXamlRule
    {
        public RuleMetadata Metadata { get; } = new(id, null, "fake", Severity.Warning, dialects, "https://example.com", false, null);
        public IEnumerable<Diagnostic> Analyze(XamlDocument doc, RuleContext ctx) => body(doc);
    }

    private sealed class FakeToolRule(string id, Func<XamlDocument, IEnumerable<Diagnostic>> body) : IToolRule
    {
        public RuleMetadata Metadata { get; } = new(id, null, "fake-tool", Severity.Error, Dialect.All, "https://example.com", false, null);
        public IEnumerable<Diagnostic> Analyze(XamlDocument doc, RuleContext ctx) => body(doc);
    }
}
