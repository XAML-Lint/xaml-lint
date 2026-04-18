namespace XamlLint.Core;

/// <summary>
/// Feature-detection layer for dialect-and-framework-version-gated XAML capabilities.
/// Rules query this rather than hard-coding <c>if (dialect == Dialect.Wpf)</c> checks, so
/// the capability matrix stays in one place and grows with the platform.
/// </summary>
/// <remarks>
/// All predicates accept a nullable <c>frameworkMajorVersion</c>. <c>null</c> means
/// "framework version unspecified" and is treated as the newest known version, which
/// matches the default we want when users haven't opted into a legacy framework.
/// </remarks>
public static class DialectFeatures
{
    /// <summary>
    /// Returns <c>true</c> when the dialect+framework combination supports the
    /// <c>RowDefinitions="Auto,*"</c> / <c>ColumnDefinitions="*,Auto"</c> shorthand
    /// attribute on a <c>&lt;Grid&gt;</c> element. WPF added this in .NET 10
    /// (<see href="https://github.com/dotnet/wpf/pull/10866"/>); every other target
    /// dialect has supported it from the start.
    /// </summary>
    public static bool SupportsGridDefinitionShorthand(Dialect dialect, int? frameworkMajorVersion) =>
        dialect switch
        {
            Dialect.Wpf => (frameworkMajorVersion ?? int.MaxValue) >= 10,
            _ => true,
        };
}
