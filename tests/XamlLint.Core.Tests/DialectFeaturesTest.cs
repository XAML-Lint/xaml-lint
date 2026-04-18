namespace XamlLint.Core.Tests;

public sealed class DialectFeaturesTest
{
    [Fact]
    public void Wpf_with_null_framework_assumes_newest_and_supports_shorthand()
    {
        DialectFeatures.SupportsGridDefinitionShorthand(Dialect.Wpf, null).Should().BeTrue();
    }

    [Fact]
    public void Wpf_net10_supports_shorthand()
    {
        DialectFeatures.SupportsGridDefinitionShorthand(Dialect.Wpf, 10).Should().BeTrue();
    }

    [Fact]
    public void Wpf_net11_supports_shorthand()
    {
        // Forward-compatible: any version >= 10.
        DialectFeatures.SupportsGridDefinitionShorthand(Dialect.Wpf, 11).Should().BeTrue();
    }

    [Fact]
    public void Wpf_net9_does_not_support_shorthand()
    {
        DialectFeatures.SupportsGridDefinitionShorthand(Dialect.Wpf, 9).Should().BeFalse();
    }

    [Fact]
    public void Wpf_net8_does_not_support_shorthand()
    {
        DialectFeatures.SupportsGridDefinitionShorthand(Dialect.Wpf, 8).Should().BeFalse();
    }

    [Theory]
    [InlineData(Dialect.WinUI3)]
    [InlineData(Dialect.Uwp)]
    [InlineData(Dialect.Maui)]
    [InlineData(Dialect.Avalonia)]
    [InlineData(Dialect.Uno)]
    public void Non_Wpf_dialects_always_support_shorthand_regardless_of_version(Dialect dialect)
    {
        DialectFeatures.SupportsGridDefinitionShorthand(dialect, null).Should().BeTrue();
        DialectFeatures.SupportsGridDefinitionShorthand(dialect, 1).Should().BeTrue();
        DialectFeatures.SupportsGridDefinitionShorthand(dialect, 9).Should().BeTrue();
        DialectFeatures.SupportsGridDefinitionShorthand(dialect, 10).Should().BeTrue();
    }
}
