using XamlLint.Core.Rules.Naming;
using XamlLint.Core.Tests.TestInfrastructure;

namespace XamlLint.Core.Tests.Rules.Naming;

public sealed class LX0302_UnusedXNameTest
{
    private const string Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";
    private const string Maui = "http://schemas.microsoft.com/dotnet/2021/maui";

    [Fact]
    public void Unused_x_Name_is_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|x:Name="Orphan"|] />
                <TextBox />
            </StackPanel>
            """);
    }

    [Fact]
    public void Name_used_via_Binding_ElementName_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox Text="{Binding ElementName=Header, Path=Content}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Name_used_via_xReference_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="Header" />
                <TextBox DataContext="{x:Reference Header}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Name_used_via_unprefixed_Reference_maui_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Label x:Name="HomeLabel" Text="Home" />
                <Entry Text="{Reference HomeLabel}" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Name_used_via_Storyboard_TargetName_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Rectangle x:Name="Box" />
                <Grid.Triggers>
                    <EventTrigger RoutedEvent="Grid.Loaded">
                        <BeginStoryboard>
                            <Storyboard>
                                <DoubleAnimation Storyboard.TargetName="Box"
                                                 Storyboard.TargetProperty="Opacity"
                                                 To="1" />
                            </Storyboard>
                        </BeginStoryboard>
                    </EventTrigger>
                </Grid.Triggers>
            </Grid>
            """);
    }

    [Fact]
    public void Name_used_via_Setter_TargetName_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <ControlTemplate xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}" TargetType="Button">
                <Border x:Name="Chrome">
                    <ContentPresenter />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="Chrome" Property="Background" Value="Red" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            """);
    }

    [Fact]
    public void Name_used_via_Trigger_SourceName_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <ControlTemplate xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}" TargetType="Button">
                <Border x:Name="Chrome">
                    <CheckBox x:Name="Toggle" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger SourceName="Toggle" Property="IsChecked" Value="True">
                        <Setter TargetName="Chrome" Property="Background" Value="Green" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            """);
    }

    [Fact]
    public void Unprefixed_Name_is_ignored_as_a_declaration()
    {
        // LX0302 only checks x:Name. Unprefixed Name= declarations are not flagged even when
        // they have no references, because they double as framework-level property values.
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}">
                <Label Name="OrphanButUnprefixed" />
                <TextBox />
            </StackPanel>
            """);
    }

    [Fact]
    public void Name_inside_ControlTemplate_referenced_from_same_template_is_not_flagged()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
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
    public void Outer_scope_name_only_referenced_from_inside_ControlTemplate_is_flagged()
    {
        // Template scopes cannot see outer names — the reference resolves to nothing, so
        // OuterLabel is unused from the outer scope's perspective.
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|x:Name="OuterLabel"|] />
                <Button>
                    <Button.Template>
                        <ControlTemplate>
                            <TextBlock Text="{Binding ElementName=OuterLabel}" />
                        </ControlTemplate>
                    </Button.Template>
                </Button>
            </StackPanel>
            """);
    }

    [Fact]
    public void XBind_path_is_not_treated_as_a_reference()
    {
        // {x:Bind Header.Text} is a typed-path binding, not an element-name reference.
        // Documented limitation — Header reports as unused here.
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|x:Name="Header"|] />
                <TextBox Text="{x:Bind Header.Text}" />
            </StackPanel>
            """,
            Dialect.WinUI3);
    }

    [Fact]
    public void Case_mismatch_reference_is_not_a_use()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|x:Name="Header"|] />
                <TextBox Text="{Binding ElementName=header}" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Multiple_unused_names_each_emit_one_diagnostic()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|x:Name="Alpha"|] />
                <Label [|x:Name="Beta"|] />
                <Label [|x:Name="Gamma"|] />
            </StackPanel>
            """);
    }

    [Fact]
    public void Empty_x_Name_value_is_ignored()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <StackPanel xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label x:Name="" />
            </StackPanel>
            """);
    }

    [Fact]
    public void Maui_dialect_flags_unused_x_Name()
    {
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <ContentPage xmlns="{{Maui}}" xmlns:x="{{Xaml2009}}">
                <Label [|x:Name="HomeLabel"|] Text="Home" />
                <Entry Text="Hello" />
            </ContentPage>
            """,
            Dialect.Maui);
    }

    [Fact]
    public void Attribute_with_Name_suffix_but_not_TargetName_does_not_keep_declaration_alive()
    {
        // DataSourceName / UserName are not XAML name-reference attributes.
        XamlDiagnosticVerifier<LX0302_UnusedXName>.Analyze(
            $$"""
            <Grid xmlns="{{Wpf}}" xmlns:x="{{Xaml2006}}">
                <Label [|x:Name="Header"|] />
                <UserControl DataSourceName="Header" UserName="Header" />
            </Grid>
            """);
    }
}
