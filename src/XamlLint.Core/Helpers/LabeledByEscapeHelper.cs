using System.Xml.Linq;
using XamlLint.Core.NameResolution;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Shared <c>AutomationProperties.LabeledBy</c> suppressor logic used by the accessibility
/// rules LX0700/LX0701/LX0702. A <c>LabeledBy</c> value suppresses the diagnostic when:
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

        // {x:Reference}, {Reference}, or {Binding ElementName=…} — validate the target in scope.
        if (ElementReference.TryParse(value, out var reference))
        {
            return context.Names.IsDefinedInScopeOf(element, reference.TargetName);
        }

        // Any other case — malformed extension, {Binding} without ElementName, StaticResource
        // and friends — can't evaluate statically; treat as "author has stated intent".
        return true;
    }
}
