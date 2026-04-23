using XamlLint.Core.Helpers;

namespace XamlLint.Core.NameResolution;

public enum ElementReferenceKind
{
    /// <summary>
    /// <c>{x:Reference Foo}</c> or <c>{Reference Foo}</c> — XAML 2009 element-reference primitive.
    /// </summary>
    XReference,

    /// <summary>
    /// <c>{Binding ElementName=Foo, …}</c> — the dominant WPF element-reference idiom.
    /// </summary>
    BindingElementName,
}

/// <summary>
/// Carries the statically-resolvable target name of an element-reference markup extension
/// together with the form it was written in. Consumed via <see cref="ElementReference.TryParse"/>.
/// </summary>
public readonly record struct ElementReferenceInfo(string TargetName, ElementReferenceKind Kind);

/// <summary>
/// Parses an attribute value that may be an element-reference markup extension into its
/// target name + kind. The three recognised forms are <c>{x:Reference X}</c>,
/// <c>{Reference X}</c>, and <c>{Binding ElementName=X, …}</c>. Any other value — literal,
/// other markup extension, <c>{Binding}</c> without <c>ElementName</c>, empty / malformed
/// reference — returns <c>false</c>.
/// </summary>
/// <remarks>
/// This is the single source of truth for "is this attribute value an element reference, and
/// if so which element does it point at?". Used by a11y suppression helpers today
/// (<c>LabeledByEscapeHelper</c>, <c>LabelTargetEscapeHelper</c>) and by the name-reference
/// validation rules (LX0202, LX0203) + unused-name rule (LX0302) in later milestones.
/// Callers resolve the target name against <see cref="XamlNameIndex"/>.
/// </remarks>
public static class ElementReference
{
    public static bool TryParse(string? attributeValue, out ElementReferenceInfo info)
    {
        info = default;
        if (string.IsNullOrWhiteSpace(attributeValue)) return false;
        if (!MarkupExtensionHelpers.IsMarkupExtension(attributeValue)) return false;
        if (!MarkupExtensionHelpers.TryParseExtension(attributeValue, out var parsed)) return false;

        if (string.Equals(parsed.Name, "x:Reference", StringComparison.Ordinal)
            || string.Equals(parsed.Name, "Reference", StringComparison.Ordinal))
        {
            var target = ReferenceTargetNameHelper.Extract(attributeValue!);
            if (string.IsNullOrWhiteSpace(target)) return false;
            info = new ElementReferenceInfo(target!, ElementReferenceKind.XReference);
            return true;
        }

        if (string.Equals(parsed.Name, "Binding", StringComparison.Ordinal))
        {
            if (parsed.NamedArguments.TryGetValue("ElementName", out var elementName)
                && !string.IsNullOrWhiteSpace(elementName))
            {
                info = new ElementReferenceInfo(elementName, ElementReferenceKind.BindingElementName);
                return true;
            }
            return false;
        }

        return false;
    }
}
