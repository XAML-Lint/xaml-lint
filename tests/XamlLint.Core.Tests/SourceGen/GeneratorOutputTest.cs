using System.Reflection;

namespace XamlLint.Core.Tests.SourceGen;

public sealed class GeneratorOutputTest
{
    [Fact]
    public void GeneratedRuleCatalog_type_exists_and_is_callable()
    {
        var asm = typeof(IXamlRule).Assembly;
        var catalogType = asm.GetType("XamlLint.Core.GeneratedRuleCatalog", throwOnError: false);

        catalogType.Should().NotBeNull("the source generator should have emitted GeneratedRuleCatalog");

        var prop = catalogType!.GetProperty("Rules", BindingFlags.Public | BindingFlags.Static);
        prop.Should().NotBeNull();

        var rules = prop!.GetValue(null) as System.Collections.IEnumerable;
        rules.Should().NotBeNull();

        // Empty until Task 7 adds rule classes; the type and property existing is what we assert here.
    }
}
