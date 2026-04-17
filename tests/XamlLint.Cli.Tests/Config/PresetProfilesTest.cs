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
}
