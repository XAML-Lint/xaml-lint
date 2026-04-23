namespace XamlLint.Core.Tests;

public sealed class CoreTypesTest
{
    [Fact]
    public void Dialect_All_is_union_of_individual_flags()
    {
        var union = Dialect.Wpf | Dialect.WinUI3 | Dialect.Uwp | Dialect.Maui | Dialect.Avalonia | Dialect.Uno;
        Dialect.All.Should().Be(union);
    }

    [Fact]
    public void Dialect_None_is_zero()
    {
        ((int)Dialect.None).Should().Be(0);
    }

    [Fact]
    public void Severity_ordering_is_info_warning_error()
    {
        ((int)Severity.Info).Should().BeLessThan((int)Severity.Warning);
        ((int)Severity.Warning).Should().BeLessThan((int)Severity.Error);
    }

    [Fact]
    public void Diagnostic_stores_all_fields_via_record_equality()
    {
        var a = new Diagnostic("LX0001", Severity.Error, "msg", "file.xaml", 1, 2, 3, 4, "https://example.com");
        var b = new Diagnostic("LX0001", Severity.Error, "msg", "file.xaml", 1, 2, 3, 4, "https://example.com");
        a.Should().Be(b);
    }

    [Fact]
    public void RuleMetadata_records_id_title_severity_dialects()
    {
        var m = new RuleMetadata(
            Id: "LX0100",
            UpstreamId: "RXT101",
            Title: "example",
            DefaultSeverity: Severity.Warning,
            Dialects: Dialect.All,
            HelpUri: "https://example.com",
            Deprecated: false,
            ReplacedBy: null);

        m.Id.Should().Be("LX0100");
        m.Dialects.Should().Be(Dialect.All);
    }

    [Fact]
    public void XamlLintCategory_for_id_maps_by_hundreds_digit()
    {
        XamlLintCategoryExtensions.ForId("LX0001").Should().Be(XamlLintCategory.Tool);
        XamlLintCategoryExtensions.ForId("LX0100").Should().Be(XamlLintCategory.Layout);
        XamlLintCategoryExtensions.ForId("LX0250").Should().Be(XamlLintCategory.Bindings);
        XamlLintCategoryExtensions.ForId("LX0300").Should().Be(XamlLintCategory.Naming);
        XamlLintCategoryExtensions.ForId("LX0499").Should().Be(XamlLintCategory.Resources);
        XamlLintCategoryExtensions.ForId("LX0500").Should().Be(XamlLintCategory.Input);
        XamlLintCategoryExtensions.ForId("LX0600").Should().Be(XamlLintCategory.Deprecated);
    }

    [Theory]
    [InlineData("LX")]        // no digits
    [InlineData("LX0")]       // too few digits
    [InlineData("LX01000")]    // too many digits
    [InlineData("XX100")]     // wrong prefix
    [InlineData("")]
    public void XamlLintCategory_for_id_rejects_malformed_ids(string id)
    {
        var act = () => XamlLintCategoryExtensions.ForId(id);
        act.Should().Throw<ArgumentException>();
    }
}
