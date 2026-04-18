using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Tests.Helpers;

public sealed class NumericRangeHelpersTest
{
    [Fact]
    public void TryReadLiteralDouble_returns_null_for_null_attribute()
    {
        NumericRangeHelpers.TryReadLiteralDouble(null).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_parses_integer_literal()
    {
        var attr = new XAttribute("Minimum", "5");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().Be(5.0);
    }

    [Fact]
    public void TryReadLiteralDouble_parses_decimal_literal_with_invariant_culture()
    {
        var attr = new XAttribute("Maximum", "3.14");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().Be(3.14);
    }

    [Fact]
    public void TryReadLiteralDouble_parses_negative_literal()
    {
        var attr = new XAttribute("Minimum", "-2.5");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().Be(-2.5);
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_markup_extension()
    {
        var attr = new XAttribute("Minimum", "{Binding Low}");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_non_numeric_literal()
    {
        var attr = new XAttribute("Minimum", "banana");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_empty_literal()
    {
        var attr = new XAttribute("Minimum", "");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }

    [Fact]
    public void TryReadLiteralDouble_returns_null_for_whitespace_literal()
    {
        var attr = new XAttribute("Minimum", "   ");
        NumericRangeHelpers.TryReadLiteralDouble(attr).Should().BeNull();
    }
}
