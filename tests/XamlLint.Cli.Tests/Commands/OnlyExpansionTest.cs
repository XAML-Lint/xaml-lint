using XamlLint.Cli.Commands;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Commands;

public sealed class OnlyExpansionTest
{
    private static readonly IReadOnlyDictionary<string, Severity> Defaults = new Dictionary<string, Severity>
    {
        ["LX100"] = Severity.Warning,
        ["LX700"] = Severity.Info,
        ["LX200"] = Severity.Error,
    };

    [Fact]
    public void Bare_id_uses_rule_default_severity()
    {
        var r = OnlyExpansion.Expand(new[] { "LX700" }, Defaults);
        r.Errors.Should().BeEmpty();
        r.PresetOverride.Should().Be("xaml-lint:off");
        r.ForceNoConfigLookup.Should().BeTrue();
        r.Severities.Should().ContainSingle().Which.Key.Should().Be("LX700");
        r.Severities["LX700"].Should().Be(Severity.Info);
    }

    [Fact]
    public void Id_with_severity_overrides_default()
    {
        var r = OnlyExpansion.Expand(new[] { "LX700:error" }, Defaults);
        r.Errors.Should().BeEmpty();
        r.Severities["LX700"].Should().Be(Severity.Error);
    }

    [Fact]
    public void Csv_and_repeated_merge()
    {
        var r = OnlyExpansion.Expand(new[] { "LX100,LX700:error", "LX200:warning" }, Defaults);
        r.Errors.Should().BeEmpty();
        r.Severities["LX100"].Should().Be(Severity.Warning);
        r.Severities["LX700"].Should().Be(Severity.Error);
        r.Severities["LX200"].Should().Be(Severity.Warning);
    }

    [Fact]
    public void Off_in_only_is_rejected()
    {
        var r = OnlyExpansion.Expand(new[] { "LX100:off" }, Defaults);
        r.Errors.Should().ContainSingle().Which.Should().Contain("off");
    }

    [Fact]
    public void Unknown_rule_id_reports_error()
    {
        var r = OnlyExpansion.Expand(new[] { "LX999" }, Defaults);
        r.Errors.Should().ContainSingle().Which.Should().Contain("LX999");
    }
}
