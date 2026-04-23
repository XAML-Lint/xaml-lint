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
}
