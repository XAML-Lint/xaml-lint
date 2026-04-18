using XamlLint.Core.Rules.Resources;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Resources;

public sealed class LX400_HardcodedStringTest
{
    [Fact]
    public void Hardcoded_Text_is_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       [|Text="Click me"|] />
            """);
    }

    [Fact]
    public void Hardcoded_Title_is_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Title="Main Window"|] />
            """);
    }

    [Fact]
    public void Hardcoded_Content_on_Button_is_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    [|Content="Save"|] />
            """);
    }

    [Fact]
    public void Binding_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="{Binding Greeting}" />
            """);
    }

    [Fact]
    public void StaticResource_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="{StaticResource GreetingText}" />
            """);
    }

    [Fact]
    public void Empty_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="" />
            """);
    }

    [Fact]
    public void Whitespace_only_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="   " />
            """);
    }

    [Fact]
    public void Attribute_not_in_scope_is_not_flagged()
    {
        // Width is not a text-presenting attribute.
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       Text="{Binding Hello}"
                       Width="100" />
            """);
    }

    [Fact]
    public void Multiple_hardcoded_strings_each_flag()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock [|Text="First"|] />
                <TextBlock [|Text="Second"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void PlaceholderText_is_flagged()
    {
        XamlDiagnosticVerifier<LX400_HardcodedString>.Analyze(
            """
            <TextBox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     [|PlaceholderText="Enter name"|] />
            """);
    }
}
