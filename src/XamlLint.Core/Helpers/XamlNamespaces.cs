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
    /// (Uno 6.0, as of 2026-04).
    /// </summary>
    /// <remarks>
    /// Additional URIs documented by Uno but not yet covered:
    /// <c>http://uno.ui/androidskia</c>, <c>http://uno.ui/iosskia</c>,
    /// <c>http://uno.ui/wasmskia</c>, and <c>http://uno.ui/netstdref</c>. These are
    /// Skia-rendering / cross-target combinations; adding them is a separate enhancement.
    /// <c>http://uno.ui/xamarin</c> was retired in Uno 5.x and is intentionally omitted.
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, string> UnoPlatformUris =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http://uno.ui/android"] = "Android",
            ["http://uno.ui/ios"]     = "iOS",
            ["http://uno.ui/wasm"]    = "WebAssembly",
            ["http://uno.ui/macos"]   = "macOS",
            ["http://uno.ui/skia"]    = "Skia",
            ["http://uno.ui/not_win"] = "NonWindows",
        };

    public static bool IsXamlNamespace(string? uri) =>
        uri == Xaml2006 || uri == Xaml2009;
}
