using XamlLint.Cli.Commands;
using XamlLint.Core;

namespace XamlLint.Cli.Tests.Commands;

public sealed class SeveritySlotParserTest
{
    [Theory]
    [InlineData("off",     true, null)]
    [InlineData("info",    true, (int)Severity.Info)]
    [InlineData("warning", true, (int)Severity.Warning)]
    [InlineData("error",   true, (int)Severity.Error)]
    [InlineData("OFF",     true, null)]
    [InlineData("Warning", true, (int)Severity.Warning)]
    public void Known_tokens_parse(string raw, bool ok, int? expectedBox)
    {
        var success = SeveritySlotParser.TryParse(raw, out var sev, out _);
        success.Should().Be(ok);
        if (expectedBox is null) sev.Should().BeNull();
        else sev.Should().Be((Severity)expectedBox);
    }

    [Theory]
    [InlineData("warn")]
    [InlineData("none")]
    [InlineData("disable")]
    public void Unknown_tokens_fail_with_message(string raw)
    {
        var success = SeveritySlotParser.TryParse(raw, out _, out var err);
        success.Should().BeFalse();
        err.Should().Contain(raw).And.Contain("off|info|warning|error");
    }

    [Fact]
    public void Empty_string_fails()
    {
        var success = SeveritySlotParser.TryParse("", out _, out var err);
        success.Should().BeFalse();
        err.Should().Contain("off|info|warning|error");
    }
}
