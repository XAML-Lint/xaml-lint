using XamlLint.Core.Rules.Bindings;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Bindings;

public sealed class LX0202_DanglingBindingElementNameTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Binding_ElementName_with_missing_target_is_flagged()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" Content="Hello" />
                <TextBox [|Text="{Binding ElementName=Ghost}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Binding_ElementName_with_existing_target_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" Content="Hello" />
                <TextBox Text="{Binding ElementName=Header, Path=Content}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Binding_without_ElementName_is_not_flagged()
    {
        // Pure data-path {Binding} is not our concern.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox Text="{Binding Path=UserName}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Binding_with_empty_ElementName_is_not_flagged()
    {
        // Nothing to dangle. Empty value is a user typo concern, not our rule.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox Text="{Binding ElementName=}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void XReference_is_not_flagged_by_LX0202()
    {
        // LX0203's job; LX0202 must ignore {x:Reference}.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <TextBox Text="{x:Reference Ghost}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void XBind_is_not_flagged_by_LX0202()
    {
        // x:Bind has no ElementName argument; it reaches named elements by typed path.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <TextBox Text="{x:Bind Ghost.Text}" />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Literal_string_value_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox Text="ElementName=Ghost" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Target_inside_same_template_scope_resolves()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <Border x:Name="Inner">
                                <TextBlock Text="{Binding ElementName=Inner, Path=Background}" />
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
        // XAML namescope: names declared outside a ControlTemplate are not visible inside.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="OuterLabel" />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <TextBlock [|Text="{Binding ElementName=OuterLabel}"|] />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
    }

    [Fact]
    public void Target_inside_DataTemplate_is_not_visible_from_root_scope()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <ListBox>
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <TextBlock x:Name="ItemText" />
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
                <TextBox [|Text="{Binding ElementName=ItemText}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Unprefixed_Name_attribute_counts_as_a_declaration()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <Label Name="Header" Content="Hello" />
                <TextBox Text="{Binding ElementName=Header, Path=Content}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Names_are_case_sensitive()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox [|Text="{Binding ElementName=header}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Multiple_dangling_bindings_each_emit_a_diagnostic()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox [|Text="{Binding ElementName=Alpha}"|] />
                <TextBox [|Text="{Binding ElementName=Beta}"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Maui_dialect_flags_dangling_ElementName()
    {
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Label x:Name="HomeLabel" Text="Home" />
                <Entry [|Text="{Binding ElementName=Ghost}"|] />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Malformed_markup_extension_is_not_flagged()
    {
        // No crash — ElementReference.TryParse returns false; rule silently passes.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox Text="{Binding ElementName=}" />
                <TextBox Text="{Binding , ElementName}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Diagnostic_span_covers_the_entire_attribute()
    {
        // Sanity check: the marker includes name, =, quotes, and value — exactly what
        // LocationHelpers.GetAttributeSpan returns. If the span shrinks to just the value
        // or expands past the quotes, the diagnostic engine will disagree with the marker
        // and this test will fail.
        XamlDiagnosticVerifier<LX0202_DanglingBindingElementName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <TextBox [|Text="{Binding ElementName=Ghost}"|] />
            </StackPanel>
            """);
    }
}
