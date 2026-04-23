using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Detects XAML attributes declared either in attribute syntax
/// (<c>&lt;TextBox InputScope="Number"/&gt;</c>) or in property-element syntax
/// (<c>&lt;TextBox&gt;&lt;TextBox.InputScope&gt;…&lt;/TextBox.InputScope&gt;&lt;/TextBox&gt;</c>).
/// Rule consumers that only call <see cref="XElement.Attribute(string)"/> miss the second
/// form and report false positives on files that use it — the upstream Rapid XAML Toolkit
/// flattens both via its <c>RapidXamlElement.HasAttribute</c> abstraction.
/// </summary>
/// <remarks>
/// Property-element names surface in XLinq as <c>TypeName.PropertyName</c>; the helper
/// matches on the trailing <c>.PropertyName</c> suffix so any <c>*.PropertyName</c> child
/// counts as a declaration. This tolerates element subclassing (e.g. a custom
/// <c>MyTextBox</c> writing <c>MyTextBox.InputScope</c>) without needing to know the
/// element's own local name.
/// </remarks>
public static class PropertyElementHelpers
{
    /// <summary>
    /// True when <paramref name="element"/> declares <paramref name="propertyName"/> as an
    /// XML attribute or as a property-element child.
    /// </summary>
    public static bool HasAttributeOrPropertyElement(XElement element, string propertyName)
    {
        if (element.Attribute(propertyName) is not null) return true;

        var suffix = "." + propertyName;
        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName.EndsWith(suffix, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the attribute value when present, otherwise the property-element's inner text
    /// when the property is written in element syntax, otherwise <c>null</c>. The attribute
    /// form wins when both are declared on the same element — mirrors how XAML parsers
    /// resolve duplicates in practice.
    /// </summary>
    public static string? GetAttributeOrPropertyElementValue(XElement element, string propertyName)
    {
        var attr = element.Attribute(propertyName);
        if (attr is not null) return attr.Value;

        var suffix = "." + propertyName;
        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName.EndsWith(suffix, StringComparison.Ordinal))
                return child.Value;
        }
        return null;
    }
}
