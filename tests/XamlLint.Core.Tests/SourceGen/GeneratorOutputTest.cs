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

        // Asserts the generated type + property exist; populated-catalog invariants live in CatalogMetaTest.
    }

    [Fact]
    public void GeneratedRuleCatalog_contains_the_tool_diagnostics()
    {
        var rules = XamlLint.Core.GeneratedRuleCatalog.Rules;
        rules.Select(r => r.Metadata.Id).Should().Contain(
            new[] { "LX001", "LX002", "LX003", "LX004", "LX005", "LX006" });
    }
}
