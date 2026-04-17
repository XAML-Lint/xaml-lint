namespace XamlLint.Plugin.Tests;

public sealed class SanityTest
{
    [Fact]
    public void Framework_is_alive()
    {
        (1 + 1).Should().Be(2);
    }
}
