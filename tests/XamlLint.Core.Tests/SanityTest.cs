namespace XamlLint.Core.Tests;

public sealed class SanityTest
{
    [Fact]
    public void Framework_is_alive()
    {
        (1 + 1).Should().Be(2);
    }

    [Fact]
    public void Runner_reports_multiple_tests()
    {
        // Second test exists so the MTP runner's "N tests passed" output
        // is unambiguously >1 — a single-test project once silently reported
        // zero on a misconfigured host. No functional assertion needed.
        true.Should().BeTrue();
    }
}
