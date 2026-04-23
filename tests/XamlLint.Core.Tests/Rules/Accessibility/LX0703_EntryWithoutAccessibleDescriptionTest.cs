using XamlLint.Core.Rules.Accessibility;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Accessibility;

public sealed class LX0703_EntryWithoutAccessibleDescriptionTest
{
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [Fact]
    public void Bare_Entry_is_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <[|Entry|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_SemanticProperties_Description_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <Entry SemanticProperties.Description="Username" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_SemanticProperties_Hint_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <Entry SemanticProperties.Hint="Enter your username." />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_AutomationProperties_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <Entry AutomationProperties.Name="Username" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_with_x_Name_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}" xmlns:x="{Xaml2009}">
                <Entry x:Name="UsernameEntry" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Entry_on_Wpf_dialect_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <StackPanel xmlns="{Wpf}">
                <Entry />
            </StackPanel>
            """,
            Dialect.Wpf);
    }

    [Fact]
    public void Non_Entry_element_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <Label Text="hello" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Suppression_with_pragma_disables_the_rule()
    {
        XamlDiagnosticVerifier<LX0703_EntryWithoutAccessibleDescription>.Analyze(
            $"""
            <ContentPage xmlns="{Maui}">
                <!-- xaml-lint disable once LX0703 -->
                <Entry />
            </ContentPage>
            """,
            Dialect.Maui);
    }
}
