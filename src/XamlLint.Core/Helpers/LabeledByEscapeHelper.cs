using System.Xml.Linq;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Shared <c>AutomationProperties.LabeledBy</c> suppressor logic used by the accessibility
/// rules LX700/LX701/LX702. A <c>LabeledBy</c> value suppresses the diagnostic when:
/// <list type="bullet">
///   <item>It's a non-extension literal (author-supplied name, honour the intent).</item>
///   <item>It's a markup extension that cannot be statically evaluated (<c>{Binding …}</c>,
///         <c>{StaticResource …}</c>, etc.).</item>
///   <item>It's <c>{x:Reference &lt;name&gt;}</c> or <c>{Reference &lt;name&gt;}</c> AND the
///         target name resolves in the current XAML name scope
///         (see <see cref="XamlLint.Core.NameResolution.XamlNameIndex"/>).</item>
/// </list>
/// A dangling or cross-scope <c>{x:Reference}</c> does NOT suppress — the dangling reference
/// is the bug the owning rule exists to catch.
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

        // Some other extension (Binding, StaticResource, etc.) — can't evaluate statically.
        if (!string.Equals(info.Name, "x:Reference", StringComparison.Ordinal)
            && !string.Equals(info.Name, "Reference", StringComparison.Ordinal))
            return true;

        // {x:Reference} — validate the target exists in the current name scope.
        var targetName = ReferenceTargetNameHelper.Extract(value);
        if (string.IsNullOrWhiteSpace(targetName)) return true; // empty/malformed — don't second-guess

        return context.Names.IsDefinedInScopeOf(element, targetName!);
    }
}
