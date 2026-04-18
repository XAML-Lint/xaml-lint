using XamlLint.Cli.Config;

namespace XamlLint.Cli.Tests.Config;

public sealed class FrameworkVersionParseTest
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("10", 10)]
    [InlineData("10.0", 10)]
    [InlineData("10.0.21", 10)]
    [InlineData("net10.0", 10)]
    [InlineData("NET10.0", 10)]
    [InlineData("9", 9)]
    [InlineData("net9.0", 9)]
    public void Parses_supported_forms(string? input, int? expected)
    {
        ConfigLoader.ParseFrameworkMajorVersion(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("ten")]
    [InlineData("v10")]
    [InlineData("netcore10")]
    [InlineData("10.x")]
    public void Throws_on_unparseable(string input)
    {
        Action act = () => ConfigLoader.ParseFrameworkMajorVersion(input);
        act.Should().Throw<FormatException>();
    }
}
