using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Tests.NameResolution;

public sealed class ElementReferenceTest
{
    [Theory]
    [InlineData("{x:Reference Foo}", "Foo", ElementReferenceKind.XReference)]
    [InlineData("{x:Reference Name=Foo}", "Foo", ElementReferenceKind.XReference)]
    [InlineData("{Reference Foo}", "Foo", ElementReferenceKind.XReference)]
    [InlineData("{Reference Name=Foo}", "Foo", ElementReferenceKind.XReference)]
    [InlineData("{Binding ElementName=Foo}", "Foo", ElementReferenceKind.BindingElementName)]
    [InlineData("{Binding ElementName=Foo, Path=Bar}", "Foo", ElementReferenceKind.BindingElementName)]
    [InlineData("{Binding Path=Bar, ElementName=Foo}", "Foo", ElementReferenceKind.BindingElementName)]
    public void TryParse_recognises_reference_forms(string value, string expectedName, ElementReferenceKind expectedKind)
    {
        ElementReference.TryParse(value, out var info).Should().BeTrue();
        info.TargetName.Should().Be(expectedName);
        info.Kind.Should().Be(expectedKind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("literal")]
    [InlineData("Foo")]
    public void TryParse_returns_false_for_non_extensions(string? value)
    {
        ElementReference.TryParse(value, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("{StaticResource Key}")]
    [InlineData("{DynamicResource Key}")]
    [InlineData("{TemplateBinding Foo}")]
    [InlineData("{x:Bind Foo}")]
    public void TryParse_returns_false_for_other_markup_extensions(string value)
    {
        ElementReference.TryParse(value, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("{Binding Foo}")]
    [InlineData("{Binding Path=Foo}")]
    [InlineData("{Binding Path=Foo, Mode=TwoWay}")]
    public void TryParse_returns_false_for_Binding_without_ElementName(string value)
    {
        ElementReference.TryParse(value, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("{Binding ElementName=}")]
    [InlineData("{Binding ElementName=   }")]
    public void TryParse_returns_false_for_Binding_with_empty_ElementName(string value)
    {
        ElementReference.TryParse(value, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("{x:Reference}")]
    [InlineData("{x:Reference }")]
    [InlineData("{Reference}")]
    public void TryParse_returns_false_for_x_Reference_without_target(string value)
    {
        ElementReference.TryParse(value, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{}")]
    [InlineData("{{escaped}")]
    public void TryParse_returns_false_for_malformed_input(string value)
    {
        ElementReference.TryParse(value, out _).Should().BeFalse();
    }

    [Fact]
    public void FindAll_returns_top_level_BindingElementName()
    {
        var refs = ElementReference.FindAll("{Binding ElementName=Header, Path=Content}").ToList();

        refs.Should().ContainSingle();
        refs[0].Kind.Should().Be(ElementReferenceKind.BindingElementName);
        refs[0].TargetName.Should().Be("Header");
    }

    [Fact]
    public void FindAll_returns_top_level_XReference_positional()
    {
        var refs = ElementReference.FindAll("{x:Reference Header}").ToList();

        refs.Should().ContainSingle();
        refs[0].Kind.Should().Be(ElementReferenceKind.XReference);
        refs[0].TargetName.Should().Be("Header");
    }

    [Fact]
    public void FindAll_returns_top_level_XReference_named_argument()
    {
        var refs = ElementReference.FindAll("{x:Reference Name=Header}").ToList();

        refs.Should().ContainSingle();
        refs[0].Kind.Should().Be(ElementReferenceKind.XReference);
        refs[0].TargetName.Should().Be("Header");
    }

    [Fact]
    public void FindAll_returns_nested_XReference_inside_Binding_Source()
    {
        // The outer Binding has no ElementName, so the top-level lookup yields nothing.
        // The inner {x:Reference Inner} is what we expect to surface.
        var refs = ElementReference.FindAll("{Binding Source={x:Reference Inner}, Path=Content}").ToList();

        refs.Should().ContainSingle();
        refs[0].Kind.Should().Be(ElementReferenceKind.XReference);
        refs[0].TargetName.Should().Be("Inner");
    }

    [Fact]
    public void FindAll_returns_outer_BindingElementName_AND_nested_XReference()
    {
        // Both refs surface — outer (Binding ElementName=Outer) and inner ({x:Reference Inner}).
        var refs = ElementReference
            .FindAll("{Binding ElementName=Outer, Source={x:Reference Inner}}")
            .ToList();

        refs.Should().HaveCount(2);
        refs.Should().Contain(r => r.Kind == ElementReferenceKind.BindingElementName && r.TargetName == "Outer");
        refs.Should().Contain(r => r.Kind == ElementReferenceKind.XReference && r.TargetName == "Inner");
    }

    [Fact]
    public void FindAll_returns_empty_for_non_extension_value()
    {
        ElementReference.FindAll("just a literal string").Should().BeEmpty();
    }

    [Fact]
    public void FindAll_returns_empty_for_null_or_whitespace()
    {
        ElementReference.FindAll(null).Should().BeEmpty();
        ElementReference.FindAll("").Should().BeEmpty();
        ElementReference.FindAll("   ").Should().BeEmpty();
    }

    [Fact]
    public void FindAll_returns_empty_for_Binding_without_ElementName()
    {
        // Pure data-path Binding has nothing to resolve at lint time.
        ElementReference.FindAll("{Binding Path=UserName}").Should().BeEmpty();
    }

    [Fact]
    public void FindAll_recurses_into_StaticResource_value_paths()
    {
        // Even unusual nestings: {Binding Source={StaticResource ...}} has no reference,
        // but {Binding Source={x:Reference Foo}, Converter={StaticResource Conv}} does.
        var refs = ElementReference
            .FindAll("{Binding Source={x:Reference Foo}, Converter={StaticResource Conv}}")
            .ToList();

        refs.Should().ContainSingle();
        refs[0].TargetName.Should().Be("Foo");
    }
}
