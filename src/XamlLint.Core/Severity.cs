namespace XamlLint.Core;

/// <summary>
/// Severity of an emitted diagnostic. Ordered Info &lt; Warning &lt; Error; users may downgrade
/// or upgrade via <c>xaml-lint.config.json</c>.
/// </summary>
public enum Severity
{
    Info = 0,
    Warning = 1,
    Error = 2,
}
