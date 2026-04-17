namespace XamlLint.Cli.Tests;

public sealed class SanityTest
{
    [Fact]
    public void Cli_assembly_can_be_referenced()
    {
        var cliAssembly = typeof(XamlLint.Cli.Program).Assembly;
        cliAssembly.Should().NotBeNull();
    }
}
