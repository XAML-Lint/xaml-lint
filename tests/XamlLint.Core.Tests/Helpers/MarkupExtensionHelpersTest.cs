using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class MarkupExtensionHelpersTest
{
    [Theory]
    [InlineData("{Binding Foo}", true)]
    [InlineData("{x:Bind Foo}", true)]
    [InlineData("{StaticResource Key}", true)]
    [InlineData("  {Binding Foo}  ", true)]
    [InlineData("literal text", false)]
    [InlineData("", false)]
    [InlineData("{", false)]
    [InlineData("}", false)]
    [InlineData("{{escaped}", false)]
    public void IsMarkupExtension_matches_braced_expressions(string value, bool expected)
    {
        MarkupExtensionHelpers.IsMarkupExtension(value).Should().Be(expected);
    }

    [Theory]
    [InlineData("{Binding}", "Binding")]
    [InlineData("{Binding Foo}", "Binding")]
    [InlineData("{Binding Foo, Mode=TwoWay}", "Binding")]
    [InlineData("{x:Bind Foo}", "x:Bind")]
    [InlineData("{x:Bind}", "x:Bind")]
    [InlineData("  {Binding Foo}", "Binding")]
    [InlineData("{TemplateBinding Foo}", "TemplateBinding")]
    [InlineData("{StaticResource Key}", "StaticResource")]
    public void TryParseExtension_returns_the_extension_name(string value, string expected)
    {
        MarkupExtensionHelpers.TryParseExtension(value, out var info).Should().BeTrue();
        info.Name.Should().Be(expected);
    }

    [Fact]
    public void TryParseExtension_returns_false_for_non_extensions()
    {
        MarkupExtensionHelpers.TryParseExtension("literal", out _).Should().BeFalse();
        MarkupExtensionHelpers.TryParseExtension("", out _).Should().BeFalse();
        MarkupExtensionHelpers.TryParseExtension("{}", out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("{Binding Foo, Mode=TwoWay}", "TwoWay")]
    [InlineData("{Binding Foo, Mode=OneWay}", "OneWay")]
    [InlineData("{Binding Foo,Mode=TwoWay}", "TwoWay")]
    [InlineData("{Binding Path=Foo, Mode = TwoWay }", "TwoWay")]
    [InlineData("{Binding Foo, Converter={StaticResource Bool}, Mode=TwoWay}", "TwoWay")]
    public void TryParseExtension_extracts_named_argument(string value, string expectedMode)
    {
        MarkupExtensionHelpers.TryParseExtension(value, out var info).Should().BeTrue();
        info.NamedArguments.TryGetValue("Mode", out var mode).Should().BeTrue();
        mode.Should().Be(expectedMode);
    }

    [Fact]
    public void TryParseExtension_missing_named_argument_returns_absent()
    {
        MarkupExtensionHelpers.TryParseExtension("{Binding Foo}", out var info).Should().BeTrue();
        info.NamedArguments.ContainsKey("Mode").Should().BeFalse();
    }

    [Fact]
    public void TryParseExtension_ignores_nested_braces_when_splitting()
    {
        // The outer comma splits arguments; the inner {StaticResource Bool} must not be
        // split at its own comma-less boundary, but Converter= must still resolve.
        MarkupExtensionHelpers.TryParseExtension(
            "{Binding Foo, Converter={StaticResource Bool}, Mode=TwoWay}",
            out var info).Should().BeTrue();
        info.NamedArguments["Mode"].Should().Be("TwoWay");
        info.NamedArguments["Converter"].Should().Be("{StaticResource Bool}");
    }
}
