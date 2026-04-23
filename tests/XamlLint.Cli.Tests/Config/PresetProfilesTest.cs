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
        foreach (var id in new[] { "LX0001", "LX0002", "LX0003", "LX0004", "LX0005", "LX0006" })
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

    // Severity / enablement matrix for the MAUI rule batch (LX0402, LX0503-LX0506, LX0601, LX0700-LX0701).
    // :recommended reflects DefaultSeverity when DefaultEnabled=true, else "off".
    // :strict auto-bumps one level (Info→warning, Warning→error).
    [Theory]
    [InlineData("LX0402", "warning", "error")]   // Resources, DefaultSeverity=Warning, DefaultEnabled=true
    [InlineData("LX0503", "info",    "warning")] // Input, Info
    [InlineData("LX0504", "warning", "error")]   // Input, Warning
    [InlineData("LX0505", "warning", "error")]   // Input, Warning
    [InlineData("LX0506", "info",    "warning")] // Input, Info
    [InlineData("LX0601", "info",    "warning")] // Deprecated, Info
    [InlineData("LX0700", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false → off in recommended
    [InlineData("LX0701", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false → off in recommended
    public void Maui_batch_rules_match_preset_matrix(string ruleId, string recommended, string strict)
    {
        var off = PresetProfiles.Load("xaml-lint:off").Rules!;
        var rec = PresetProfiles.Load("xaml-lint:recommended").Rules!;
        var str = PresetProfiles.Load("xaml-lint:strict").Rules!;

        off[ruleId].GetString().Should().Be("off", $"{ruleId} must be 'off' in xaml-lint:off");
        rec[ruleId].GetString().Should().Be(recommended, $"{ruleId} must be '{recommended}' in xaml-lint:recommended");
        str[ruleId].GetString().Should().Be(strict, $"{ruleId} must be '{strict}' in xaml-lint:strict");
    }

    // Severity / enablement matrix for LX0702, LX0703, LX0800.
    // :recommended reflects DefaultSeverity when DefaultEnabled=true, else "off".
    // :strict auto-bumps one level (Info→warning, Warning→error).
    [Theory]
    [InlineData("LX0702", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false
    [InlineData("LX0703", "off",     "warning")] // Accessibility, Info, DefaultEnabled=false
    [InlineData("LX0800", "warning", "error")]   // Platform, Warning, DefaultEnabled=true
    public void LX0702_LX0703_LX0800_match_preset_matrix(string ruleId, string recommended, string strict)
    {
        var off = PresetProfiles.Load("xaml-lint:off").Rules!;
        var rec = PresetProfiles.Load("xaml-lint:recommended").Rules!;
        var str = PresetProfiles.Load("xaml-lint:strict").Rules!;

        off[ruleId].GetString().Should().Be("off", $"{ruleId} must be 'off' in xaml-lint:off");
        rec[ruleId].GetString().Should().Be(recommended, $"{ruleId} must be '{recommended}' in xaml-lint:recommended");
        str[ruleId].GetString().Should().Be(strict, $"{ruleId} must be '{strict}' in xaml-lint:strict");
    }
}
