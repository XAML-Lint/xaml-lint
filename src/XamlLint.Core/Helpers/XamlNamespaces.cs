namespace XamlLint.Core.Helpers;

/// <summary>
/// Well-known XAML namespace URIs. The 2006 URI is used by WPF, UWP, WinUI 3, Avalonia, and Uno
/// for the <c>x:</c> prefix; .NET MAUI uses the 2009 revision. The <c>Ns</c> constants are the
/// raw URIs; prefer <see cref="IsXamlNamespace(string)"/> for predicate logic so new XAML
/// revisions can be added in one place.
/// </summary>
public static class XamlNamespaces
{
    public const string Xaml2006 = "http://schemas.microsoft.com/winfx/2006/xaml";
    public const string Xaml2009 = "http://schemas.microsoft.com/winfx/2009/xaml";

    public const string WpfPresentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    public static bool IsXamlNamespace(string? uri) =>
        uri == Xaml2006 || uri == Xaml2009;
}
