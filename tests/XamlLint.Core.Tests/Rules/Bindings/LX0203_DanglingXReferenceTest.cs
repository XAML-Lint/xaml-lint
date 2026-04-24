using XamlLint.Core.Rules.Bindings;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Bindings;

public sealed class LX0203_DanglingXReferenceTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void XReference_with_missing_target_is_flagged_positional_form()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox [|Text="{x:Reference Ghost}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void XReference_with_missing_target_is_flagged_named_form()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox [|Text="{x:Reference Name=Ghost}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Unprefixed_Reference_with_missing_target_is_flagged()
    {
        // XAML 2009: "x" may be the default namespace, so {Reference Foo} is valid too.
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Label x:Name="HomeLabel" Text="Home" />
                <Entry [|Text="{Reference Ghost}"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void XReference_with_existing_target_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox Text="{x:Reference Header}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Binding_ElementName_is_not_flagged_by_LX0203()
    {
        // LX0202's job.
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox Text="{Binding ElementName=Ghost}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Target_inside_same_template_scope_resolves()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Border x:Name="Inner">
                                <TextBlock DataContext="{x:Reference Inner}" />
                            </Border>
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
    }

    [Fact]
    public void Outer_scope_target_is_not_visible_from_inside_ControlTemplate()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="OuterLabel" />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <TextBlock [|DataContext="{x:Reference OuterLabel}"|] />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
    }

    [Fact]
    public void Target_inside_DataTemplate_is_not_visible_from_root_scope()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <ListBox>
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <TextBlock x:Name="ItemText" />
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
                <TextBox [|DataContext="{x:Reference ItemText}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Unprefixed_Name_attribute_counts_as_a_declaration()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label Name="Header" />
                <TextBox DataContext="{x:Reference Header}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Names_are_case_sensitive()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox [|DataContext="{x:Reference header}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Label_Target_xReference_to_missing_element_is_flagged()
    {
        // Realistic WPF pattern: Label.Target="{x:Reference …}".
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|Target="{x:Reference Missing}"|] Content="_User" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Multiple_dangling_references_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <TextBox [|DataContext="{x:Reference Alpha}"|] />
                <TextBox [|DataContext="{x:Reference Beta}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Maui_dialect_flags_dangling_xReference()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Label x:Name="HomeLabel" Text="Home" />
                <Entry [|Text="{x:Reference Ghost}"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Empty_xReference_target_is_not_flagged()
    {
        // Malformed — ElementReference.TryParse returns false; rule silently passes.
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <TextBox Text="{x:Reference}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Literal_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox Text="x:Reference Ghost" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Quoted_positional_target_resolves_to_declared_element()
    {
        // Argument-value quoting: {x:Reference 'Foo'} is equivalent to {x:Reference Foo}.
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox DataContext="{x:Reference 'Header'}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Quoted_named_target_resolves_to_declared_element()
    {
        XamlDiagnosticVerifier<LX0203_DanglingXReference>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox DataContext="{x:Reference Name='Header'}" />
            </StackPanel>
            """);
    }
}
