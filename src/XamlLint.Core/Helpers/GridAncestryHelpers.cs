using System.Globalization;
using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Grid-layout introspection for LX100–LX103. Walks the XML tree to find the nearest
/// <c>&lt;Grid&gt;</c> ancestor, counts its declared rows/columns (both element-syntax and
/// the WinUI/UWP <c>RowDefinitions="Auto,*,..."</c> shorthand), and reads integer attached
/// properties like <c>Grid.Row</c> in either attribute or element syntax.
/// </summary>
public static class GridAncestryHelpers
{
    public const string GridElementName = "Grid";
    public const string RowDefinitionsPropertyElement = "Grid.RowDefinitions";
    public const string ColumnDefinitionsPropertyElement = "Grid.ColumnDefinitions";
    public const string RowDefinitionElement = "RowDefinition";
    public const string ColumnDefinitionElement = "ColumnDefinition";
    public const string RowDefinitionsShorthandAttribute = "RowDefinitions";
    public const string ColumnDefinitionsShorthandAttribute = "ColumnDefinitions";

    /// <summary>
    /// Value carrier for <see cref="TryReadIntegerAttachedProperty"/>. <paramref name="Source"/>
    /// is either an <see cref="XAttribute"/> (attribute syntax) or an <see cref="XElement"/>
    /// (element syntax) that the caller can hand to <c>LocationHelpers</c> to compute a span.
    /// </summary>
    public readonly record struct AttachedPropertyValue(int Value, XObject Source);

    /// <summary>
    /// Returns the first ancestor whose <c>Name.LocalName == "Grid"</c>, or <c>null</c>. The
    /// check is by local name only — every target dialect names the type <c>Grid</c> and none
    /// carries a dialect-specific prefix.
    /// </summary>
    /// <remarks>
    /// Traversal is unconstrained: it walks <see cref="XElement.Parent"/> all the way to the
    /// document root without stopping at template boundaries. A control placed inside a
    /// <c>&lt;DataTemplate&gt;</c> or <c>&lt;ControlTemplate&gt;</c> whose root is itself
    /// inside a <c>&lt;Grid&gt;</c> will claim the outer Grid as its nearest ancestor, even
    /// though at runtime the control is instantiated inside the template — the outer Grid is
    /// not its real parent. Today's rules (LX100–LX103) rarely hit this edge in practice; a
    /// future grid-aware rule that consults the ancestor's layout state should add its own
    /// template-boundary filter or wait for this helper to grow one.
    /// </remarks>
    public static XElement? FindNearestGridAncestor(XElement element)
    {
        for (var ancestor = element.Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor.Name.LocalName == GridElementName)
                return ancestor;
        }
        return null;
    }

    /// <summary>
    /// Returns the number of rows the Grid declares. When <paramref name="shorthandSupported"/>
    /// is <c>true</c> (the default) the WinUI/UWP/MAUI/Avalonia/Uno (and WPF on .NET 10+)
    /// shorthand attribute <c>RowDefinitions="Auto,*,..."</c> takes precedence; otherwise the
    /// shorthand attribute is ignored. Falls back to counting <c>&lt;RowDefinition&gt;</c>
    /// children inside <c>&lt;Grid.RowDefinitions&gt;</c>. A Grid that declares neither has an
    /// implicit single row — returns 1.
    /// </summary>
    public static int CountRowDefinitions(XElement grid, bool shorthandSupported = true) =>
        CountDefinitions(grid, RowDefinitionsShorthandAttribute, RowDefinitionsPropertyElement, RowDefinitionElement, shorthandSupported);

    /// <summary>
    /// Returns the number of columns the Grid declares. When <paramref name="shorthandSupported"/>
    /// is <c>true</c> (the default) the WinUI/UWP/MAUI/Avalonia/Uno (and WPF on .NET 10+)
    /// shorthand attribute <c>ColumnDefinitions="*,Auto,..."</c> takes precedence; otherwise the
    /// shorthand attribute is ignored. Falls back to counting <c>&lt;ColumnDefinition&gt;</c>
    /// children inside <c>&lt;Grid.ColumnDefinitions&gt;</c>. A Grid that declares neither has an
    /// implicit single column — returns 1.
    /// </summary>
    public static int CountColumnDefinitions(XElement grid, bool shorthandSupported = true) =>
        CountDefinitions(grid, ColumnDefinitionsShorthandAttribute, ColumnDefinitionsPropertyElement, ColumnDefinitionElement, shorthandSupported);

    private static int CountDefinitions(
        XElement grid,
        string shorthandAttributeName,
        string propertyElementName,
        string definitionElementName,
        bool shorthandSupported)
    {
        if (shorthandSupported)
        {
            var shorthand = grid.Attribute(shorthandAttributeName);
            if (shorthand is not null)
                return CountCommaSeparated(shorthand.Value);
        }

        var propertyElement = grid.Elements().FirstOrDefault(
            e => e.Name.LocalName == propertyElementName);
        if (propertyElement is null)
            return 1;  // implicit single row/column

        var count = propertyElement.Elements().Count(
            e => e.Name.LocalName == definitionElementName);

        // An empty <Grid.RowDefinitions /> collection behaves identically to an absent one — WPF,
        // WinUI, and the rest of the target dialects fall back to a single implicit row.
        return count == 0 ? 1 : count;
    }

    /// <summary>
    /// Reads an integer attached property (e.g. <c>Grid.Row</c>) in either attribute syntax
    /// (<c>&lt;Button Grid.Row="1" /&gt;</c>) or element syntax
    /// (<c>&lt;Button&gt;&lt;Grid.Row&gt;1&lt;/Grid.Row&gt;&lt;/Button&gt;</c>). Returns the
    /// integer value plus the source <see cref="XObject"/> for span computation, or
    /// <c>null</c> when the property is absent, its value contains non-digit characters
    /// (e.g. a markup-extension value like <c>{Binding Idx}</c>), or the value has a leading
    /// sign (<c>"-1"</c>, <c>"+0"</c>) — attached-property indexes are unsigned integers.
    /// </summary>
    public static AttachedPropertyValue? TryReadIntegerAttachedProperty(
        XElement element,
        string propertyName)
    {
        var attr = element.Attribute(propertyName);
        if (attr is not null)
        {
            return int.TryParse(attr.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var attrValue)
                ? new AttachedPropertyValue(attrValue, attr)
                : null;
        }

        var elementChild = element.Elements().FirstOrDefault(e => e.Name.LocalName == propertyName);
        if (elementChild is not null)
        {
            return int.TryParse(elementChild.Value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var elemValue)
                ? new AttachedPropertyValue(elemValue, elementChild)
                : null;
        }

        return null;
    }

    private static int CountCommaSeparated(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var count = 0;
        foreach (var part in value.Split(','))
            if (!string.IsNullOrWhiteSpace(part)) count++;
        return count;
    }
}
