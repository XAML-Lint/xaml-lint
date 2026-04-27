using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX0704",
    Title = "Icon button lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0704.md")]
public sealed partial class LX0704_IconButtonWithoutAccessibleDescription : IXamlRule
{
    private static readonly string[] PresenceEscapeAttributes =
    {
        "AutomationProperties.Name",
        "AutomationProperties.HelpText",
        "SemanticProperties.Description",
        "SemanticProperties.Hint",
        "AutomationId",
    };

    private const string IsInAccessibleTreeAttribute = "AutomationProperties.IsInAccessibleTree";

    private static readonly HashSet<string> ButtonElementLocalNames = new(StringComparer.Ordinal)
    {
        "Button",
        "ImageButton",
    };

    private static readonly HashSet<string> IconElementLocalNames = new(StringComparer.Ordinal)
    {
        "Image",
        "FontIcon",
        "SymbolIcon",
        "PathIcon",
        "BitmapIcon",
        "ImageIcon",
        "Path",
    };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (!ButtonElementLocalNames.Contains(element.Name.LocalName)) continue;
            if (HasAnyEscape(element, context)) continue;
            if (!IsIconOrSymbolContent(element)) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Icon-only button has no accessibility description; screen readers cannot convey its purpose. Set AutomationProperties.Name (or another suppressor) to describe the action.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static bool IsIconOrSymbolContent(XElement element)
    {
        // (a) Content="..." attribute: empty -> empty button; non-empty -> check for symbolic content.
        var contentAttr = element.Attribute("Content");
        if (contentAttr is not null)
        {
            var value = contentAttr.Value;
            if (string.IsNullOrWhiteSpace(value)) return true;
            return SymbolGlyphHelper.IsSymbolOrGlyph(value);
        }

        // (b) No Content attribute. Inspect element children — property-element nodes
        // (<Button.Style>, <Button.Resources>) are not layout content.
        XElement? onlyLayoutChild = null;
        var layoutChildCount = 0;
        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName.Contains('.')) continue;
            layoutChildCount++;
            if (layoutChildCount > 1) return false;  // complex content, not icon-only
            onlyLayoutChild = child;
        }

        if (layoutChildCount == 0) return true;  // empty button

        return IconElementLocalNames.Contains(onlyLayoutChild!.Name.LocalName);
    }

    private static bool HasAnyEscape(XElement element, RuleContext context)
    {
        foreach (var name in PresenceEscapeAttributes)
        {
            if (element.Attribute(name) is not null) return true;
        }

        if (LabeledByEscapeHelper.Suppresses(element, context)) return true;

        var treeAttr = element.Attribute(IsInAccessibleTreeAttribute);
        if (treeAttr is not null)
        {
            var value = treeAttr.Value;
            if (MarkupExtensionHelpers.IsMarkupExtension(value)) return true;
            if (string.Equals(value.Trim(), "False", StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}
