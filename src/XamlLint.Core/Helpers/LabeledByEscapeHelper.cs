using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Shared <c>AutomationProperties.LabeledBy</c> suppressor logic used by the accessibility
/// rules LX700/LX701/LX702. A <c>LabeledBy</c> value suppresses the diagnostic when:
/// <list type="bullet">
///   <item>It's a non-extension literal (author-supplied name, honour the intent).</item>
///   <item>It's <c>{x:Reference &lt;name&gt;}</c> / <c>{Reference &lt;name&gt;}</c> OR
///         <c>{Binding ElementName=&lt;name&gt;}</c> AND the target name resolves in the
///         current XAML name scope (see
///         <see cref="XamlLint.Core.NameResolution.XamlNameIndex"/>).
///         These are the two statically-resolvable element-reference forms in XAML:
///         <c>x:Reference</c> is the XAML 2009 primitive used on UWP / WinUI 3 / UWP-style
///         markup; <c>{Binding ElementName=…}</c> is the dominant WPF idiom.</item>
///   <item>It's a <c>{Binding …}</c> without <c>ElementName</c> (pure data binding) or any
///         other markup extension that can't be statically evaluated
///         (<c>{StaticResource …}</c>, etc.) — treated as "the author has stated intent".</item>
/// </list>
/// A dangling or cross-scope reference (<c>{x:Reference}</c> or
/// <c>{Binding ElementName=…}</c>) does NOT suppress — the dangling reference is the bug
/// the owning rule exists to catch.
/// </summary>
public static class LabeledByEscapeHelper
{
    private const string LabeledByAttribute = "AutomationProperties.LabeledBy";

    public static bool Suppresses(XElement element, RuleContext context)
    {
        var labeledBy = element.Attribute(LabeledByAttribute);
        if (labeledBy is null) return false;
        var value = labeledBy.Value;
        if (string.IsNullOrWhiteSpace(value)) return false;

        // Non-extension literal — honour the author's stated intent.
        if (!MarkupExtensionHelpers.IsMarkupExtension(value)) return true;

        // Malformed extension — don't second-guess.
        if (!MarkupExtensionHelpers.TryParseExtension(value, out var info)) return true;

        // {x:Reference} / {Reference} — XAML 2009 element-reference primitive.
        if (string.Equals(info.Name, "x:Reference", StringComparison.Ordinal)
            || string.Equals(info.Name, "Reference", StringComparison.Ordinal))
        {
            var targetName = ReferenceTargetNameHelper.Extract(value);
            if (string.IsNullOrWhiteSpace(targetName)) return true; // empty/malformed — don't second-guess
            return context.Names.IsDefinedInScopeOf(element, targetName!);
        }

        // {Binding ElementName=…} — classic WPF element-reference idiom, statically
        // resolvable just like {x:Reference}. Binding without ElementName is a pure
        // data binding and is treated as permissively suppressing (unchanged behaviour).
        if (string.Equals(info.Name, "Binding", StringComparison.Ordinal))
        {
            if (info.NamedArguments.TryGetValue("ElementName", out var elementName)
                && !string.IsNullOrWhiteSpace(elementName))
            {
                return context.Names.IsDefinedInScopeOf(element, elementName);
            }
            return true;
        }

        // Some other extension (StaticResource, etc.) — can't evaluate statically.
        return true;
    }
}
