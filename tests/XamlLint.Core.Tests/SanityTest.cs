namespace XamlLint.Core.Tests;

public sealed class SanityTest
{
    [Fact]
    public void Framework_is_alive()
    {
        (1 + 1).Should().Be(2);
    }

    [Fact]
    public void Core_assembly_can_be_referenced()
    {
        // XamlLint.Core has no types yet; M1 adds them.
        // This confirms the project reference resolves so M1 tests
        // don't need to re-prove the wiring.
        var coreAssembly = typeof(XamlLint.Core.Tests.SanityTest).Assembly;
        coreAssembly.Should().NotBeNull();
    }
}
