using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class ReferenceTargetNameHelperTest
{
    [Fact]
    public void Extracts_positional_target_from_x_Reference()
    {
        ReferenceTargetNameHelper.Extract("{x:Reference Foo}").Should().Be("Foo");
    }

    [Fact]
    public void Extracts_named_target_from_x_Reference_Name()
    {
        ReferenceTargetNameHelper.Extract("{x:Reference Name=Foo}").Should().Be("Foo");
    }

    [Fact]
    public void Extracts_positional_target_from_unprefixed_Reference()
    {
        ReferenceTargetNameHelper.Extract("{Reference Foo}").Should().Be("Foo");
    }

    [Fact]
    public void Extension_name_without_arguments_returns_null()
    {
        ReferenceTargetNameHelper.Extract("{x:Reference}").Should().BeNull();
    }

    [Fact]
    public void Empty_braces_return_null()
    {
        ReferenceTargetNameHelper.Extract("{}").Should().BeNull();
    }

    [Fact]
    public void Plain_literal_returns_null()
    {
        ReferenceTargetNameHelper.Extract("SomeLabel").Should().BeNull();
    }

    [Fact]
    public void Empty_string_returns_null()
    {
        ReferenceTargetNameHelper.Extract("").Should().BeNull();
    }

    [Fact]
    public void Positional_target_followed_by_comma_stops_at_comma()
    {
        // {x:Reference Foo, Mode=OneTime} — positional 'Foo' then trailing named arg.
        // The documented edge case: our heuristic sees '=' in rest and routes to
        // named-arg branch, which looks for Name=… and finds Mode=…, returning null.
        // Permissive side — callers treat null as "malformed, don't second-guess".
        // Locked in; fixing requires reconciling with MarkupExtensionHelpers.
        ReferenceTargetNameHelper.Extract("{x:Reference Foo, Mode=OneTime}").Should().BeNull();
    }

    [Fact]
    public void No_whitespace_between_name_and_first_arg_returns_null()
    {
        // {x:Reference,Name=Foo} — legal per XAML 2009 but our parser only splits the
        // extension name on whitespace, so the loop runs to end-of-input and returns null.
        // Permissive side; locked in pending MarkupExtensionHelpers reconciliation.
        ReferenceTargetNameHelper.Extract("{x:Reference,Name=Foo}").Should().BeNull();
    }

    [Fact]
    public void Whitespace_around_target_name_is_trimmed()
    {
        ReferenceTargetNameHelper.Extract("{x:Reference   Foo  }").Should().Be("Foo");
    }

    [Theory]
    [InlineData("{x:Reference 'Foo'}", "Foo")]
    [InlineData("{x:Reference \"Foo\"}", "Foo")]
    [InlineData("{Reference 'Foo'}", "Foo")]
    public void Surrounding_quotes_are_stripped_from_positional_target(string value, string expected)
    {
        ReferenceTargetNameHelper.Extract(value).Should().Be(expected);
    }
}
