using XamlLint.Core.Suppressions;

namespace XamlLint.Core.Tests.Suppressions;

public sealed class PragmaParserTest
{
    private static XamlDocument Parse(string src) =>
        XamlDocument.FromString(src, "inline.xaml", Dialect.Wpf);

    [Fact]
    public void Empty_document_yields_empty_map_and_no_diagnostics()
    {
        var doc = Parse("<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />");
        var result = PragmaParser.Parse(doc);

        result.Map.IsEmpty.Should().BeTrue();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Disable_adds_range_to_end_of_file_when_no_restore()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disable LX0100 -->
                <Button />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Diagnostics.Should().BeEmpty();
        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();
        result.Map.IsSuppressed("LX0100", line: 4).Should().BeTrue();
        result.Map.IsSuppressed("LX0101", line: 3).Should().BeFalse();
    }

    [Fact]
    public void Disable_restore_bounds_the_range()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disable LX0100 -->
                <A />
                <!-- xaml-lint restore LX0100 -->
                <B />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();
        result.Map.IsSuppressed("LX0100", line: 5).Should().BeFalse();
    }

    [Fact]
    public void Disable_once_covers_next_element_only()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disable once LX0100 -->
                <A />
                <B />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();  // <A />
        result.Map.IsSuppressed("LX0100", line: 4).Should().BeFalse(); // <B />
    }

    [Fact]
    public void Disable_all_suppresses_every_rule()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disable All -->
                <Button />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();
        result.Map.IsSuppressed("LX0999", line: 3).Should().BeTrue();
    }

    [Fact]
    public void Multiple_rule_ids_on_one_pragma()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disable LX0100 LX0200 -->
                <A />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();
        result.Map.IsSuppressed("LX0200", line: 3).Should().BeTrue();
        result.Map.IsSuppressed("LX0300", line: 3).Should().BeFalse();
    }

    [Fact]
    public void Malformed_pragma_emits_LX0002_at_comment_line()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disableonce LX0100 -->
                <A />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Diagnostics.Should().ContainSingle(d => d.RuleId == "LX0002");
    }

    [Fact]
    public void Non_xaml_lint_comments_are_ignored()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- ordinary comment -->
                <!-- TODO: fix me -->
                <A />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Disable_once_covers_multi_line_element_including_closing_tag()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-lint disable once LX0100 -->
                <A>
                    <B />
                </A>
                <C />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();  // <A>
        result.Map.IsSuppressed("LX0100", line: 4).Should().BeTrue();  // <B />
        result.Map.IsSuppressed("LX0100", line: 5).Should().BeTrue();  // </A>
        result.Map.IsSuppressed("LX0100", line: 6).Should().BeFalse(); // <C />
    }

    [Fact]
    public void Tabs_between_tokens_are_accepted()
    {
        const string src = "<Root xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\n" +
                           "\t<!--\txaml-lint\tdisable\tLX0100\t-->\n" +
                           "\t<A />\n" +
                           "</Root>";
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Diagnostics.Should().BeEmpty();
        result.Map.IsSuppressed("LX0100", line: 3).Should().BeTrue();
    }

    [Fact]
    public void Comment_with_xaml_lint_prefix_but_no_boundary_is_not_a_pragma()
    {
        const string src = """
            <Root xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <!-- xaml-linting is a word, not a directive -->
                <!-- xaml-lint-something -->
                <A />
            </Root>
            """;
        var doc = Parse(src);
        var result = PragmaParser.Parse(doc);

        result.Diagnostics.Should().BeEmpty();
        result.Map.IsEmpty.Should().BeTrue();
    }
}
