namespace XamlLint.Core;

/// <summary>
/// XAML dialects a rule may apply to. Use bitwise OR in <see cref="XamlRuleAttribute.Dialects"/>
/// to declare multi-dialect support.
/// </summary>
[Flags]
public enum Dialect
{
    None     = 0,
    Wpf      = 1 << 0,
    WinUI3   = 1 << 1,
    Uwp      = 1 << 2,
    Maui     = 1 << 3,
    Avalonia = 1 << 4,
    Uno      = 1 << 5,
    All      = Wpf | WinUI3 | Uwp | Maui | Avalonia | Uno,
}
