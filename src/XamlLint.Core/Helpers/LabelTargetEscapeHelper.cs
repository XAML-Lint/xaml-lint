using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Reverse-direction labeling detection used by accessibility rules: a
/// <c>&lt;Label Target="{x:Reference Foo}"&gt;…&lt;/Label&gt;</c> or
/// <c>&lt;Label Target="{Binding ElementName=Foo}"&gt;…&lt;/Label&gt;</c> provides the AT
/// name for the element named <c>Foo</c> at runtime (WPF's <c>Label.Target</c> wires the
/// automation peer). When any such Label targets the element under inspection — resolved
/// in the Label's XAML name scope — the a11y rule should suppress.
/// </summary>
/// <remarks>
/// This is specific to WPF's <c>Label.Target</c> idiom. MAUI / WinUI 3 / UWP / Avalonia /
/// Uno don't use this pattern, but the check is a safe no-op on those dialects: either
/// there are no <c>Label</c> elements, or their <c>Target</c> resolves to an element in a
/// scope that doesn't reach the inspected control.
/// </remarks>
public static class LabelTargetEscapeHelper
{
    private const string TargetAttribute = "Target";
    private const string LabelLocalName = "Label";

    /// <summary>
    /// Returns true when some <c>&lt;Label Target="…"&gt;</c> in the document targets
    /// <paramref name="element"/> via a statically-resolvable element reference — either
    /// <c>{x:Reference}</c> or <c>{Binding ElementName=…}</c> — in the Label's name scope.
    /// </summary>
    public static bool Suppresses(XElement element, RuleContext context)
    {
        var root = element.Document?.Root;
        if (root is null) return false;

        foreach (var label in root.DescendantsAndSelf())
        {
            if (label.Name.LocalName != LabelLocalName) continue;
            var targetAttr = label.Attribute(TargetAttribute);
            if (targetAttr is null) continue;

            var targetName = ExtractElementReferenceName(targetAttr.Value);
            if (string.IsNullOrWhiteSpace(targetName)) continue;

            var resolved = context.Names.ResolveInScopeOf(label, targetName!);
            if (ReferenceEquals(resolved, element)) return true;
        }

        return false;
    }

    /// <summary>
    /// Extracts the target element name from a <c>{x:Reference Foo}</c>,
    /// <c>{Reference Foo}</c>, or <c>{Binding ElementName=Foo}</c> markup-extension value.
    /// Returns null for literals, non-reference extensions, or bindings without
    /// <c>ElementName</c>.
    /// </summary>
    private static string? ExtractElementReferenceName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!MarkupExtensionHelpers.IsMarkupExtension(value)) return null;
        if (!MarkupExtensionHelpers.TryParseExtension(value, out var info)) return null;

        if (string.Equals(info.Name, "x:Reference", StringComparison.Ordinal)
            || string.Equals(info.Name, "Reference", StringComparison.Ordinal))
        {
            return ReferenceTargetNameHelper.Extract(value);
        }

        if (string.Equals(info.Name, "Binding", StringComparison.Ordinal))
        {
            return info.NamedArguments.TryGetValue("ElementName", out var elementName)
                && !string.IsNullOrWhiteSpace(elementName)
                ? elementName
                : null;
        }

        return null;
    }
}
