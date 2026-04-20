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

    /// <summary>
    /// Markup Compatibility namespace — home of the <c>mc:Ignorable</c> attribute used to mark
    /// XAML prefixes as safe to skip on platforms that don't recognise them.
    /// </summary>
    public const string MarkupCompatibility = "http://schemas.openxmlformats.org/markup-compatibility/2006";

    /// <summary>
    /// Uno Platform platform-specific XAML namespace URIs. Any <c>xmlns:</c> declaration that
    /// points at one of these URIs must appear in the root element's <c>mc:Ignorable</c> list,
    /// so XAML parsers on non-Uno runtimes can skip the prefixed attributes instead of failing
    /// at load time. Source: https://platform.uno/docs/articles/platform-specific-xaml.html
    /// (as of 2026-04).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> UnoPlatformUris =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http://uno.ui/android"] = "Android",
            ["http://uno.ui/ios"]     = "iOS",
            ["http://uno.ui/wasm"]    = "WebAssembly",
            ["http://uno.ui/macos"]   = "macOS",
            ["http://uno.ui/skia"]    = "Skia",
        };

    public static bool IsXamlNamespace(string? uri) =>
        uri == Xaml2006 || uri == Xaml2009;
}
