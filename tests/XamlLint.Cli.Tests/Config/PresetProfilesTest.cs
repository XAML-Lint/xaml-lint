using XamlLint.Cli.Config;

namespace XamlLint.Cli.Tests.Config;

public sealed class PresetProfilesTest
{
    [Theory]
    [InlineData("xaml-lint:off")]
    [InlineData("xaml-lint:recommended")]
    [InlineData("xaml-lint:strict")]
    public void Each_preset_loads_and_contains_six_tool_diagnostics(string name)
    {
        var doc = PresetProfiles.Load(name);
        doc.Rules.Should().NotBeNull();
        foreach (var id in new[] { "LX001", "LX002", "LX003", "LX004", "LX005", "LX006" })
        {
            doc.Rules!.ContainsKey(id).Should().BeTrue($"preset {name} must include {id}");
        }
    }

    [Fact]
    public void Unknown_preset_name_throws()
    {
        var act = () => PresetProfiles.Load("xaml-lint:unknown");
        act.Should().Throw<ArgumentException>();
    }

    // Severity / enablement matrix for the MAUI rule batch (LX402, LX503-LX506, LX601, LX700-LX701).
    // :recommended reflects DefaultSeverity when DefaultEnabled=true, else "off".
    // :strict auto-bumps one level (Info→warning, Warning→error).
    [Theory]
    [InlineData("LX402", "warning", "error")]   // Resources, DefaultSeverity=Warning, DefaultEnabled=true
    [InlineData("LX503", "info",    "warning")] // Input, Info
    [InlineData("LX504", "warning", "error")]   // Input, Warning
    [InlineData("LX505", "warning", "error")]   // Input, Warning
    [InlineData("LX506", "info",    "warning")] // Input, Info
    [InlineData("LX601", "info",    "warning")] // Deprecated, Info
    [InlineData("LX700", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false → off in recommended
    [InlineData("LX701", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false → off in recommended
    public void Maui_batch_rules_match_preset_matrix(string ruleId, string recommended, string strict)
    {
        var off = PresetProfiles.Load("xaml-lint:off").Rules!;
        var rec = PresetProfiles.Load("xaml-lint:recommended").Rules!;
        var str = PresetProfiles.Load("xaml-lint:strict").Rules!;

        off[ruleId].GetString().Should().Be("off", $"{ruleId} must be 'off' in xaml-lint:off");
        rec[ruleId].GetString().Should().Be(recommended, $"{ruleId} must be '{recommended}' in xaml-lint:recommended");
        str[ruleId].GetString().Should().Be(strict, $"{ruleId} must be '{strict}' in xaml-lint:strict");
    }

    // Severity / enablement matrix for LX702, LX703, LX800.
    // :recommended reflects DefaultSeverity when DefaultEnabled=true, else "off".
    // :strict auto-bumps one level (Info→warning, Warning→error).
    [Theory]
    [InlineData("LX702", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false
    [InlineData("LX703", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false
    [InlineData("LX800", "warning", "error")]   // Platform, Warning, DefaultEnabled=true
    public void LX702_LX703_LX800_match_preset_matrix(string ruleId, string recommended, string strict)
    {
        var off = PresetProfiles.Load("xaml-lint:off").Rules!;
        var rec = PresetProfiles.Load("xaml-lint:recommended").Rules!;
        var str = PresetProfiles.Load("xaml-lint:strict").Rules!;

        off[ruleId].GetString().Should().Be("off", $"{ruleId} must be 'off' in xaml-lint:off");
        rec[ruleId].GetString().Should().Be(recommended, $"{ruleId} must be '{recommended}' in xaml-lint:recommended");
        str[ruleId].GetString().Should().Be(strict, $"{ruleId} must be '{strict}' in xaml-lint:strict");
    }
}
