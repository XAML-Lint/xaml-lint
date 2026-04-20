using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Accessibility;

[XamlRule(
    Id = "LX702",
    UpstreamId = "RXT601",
    Title = "TextBox lacks accessibility description",
    DefaultSeverity = Severity.Info,
    DefaultEnabled = false,
    Dialects = Dialect.Wpf | Dialect.WinUI3 | Dialect.Uwp | Dialect.Avalonia | Dialect.Uno,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX702.md")]
public sealed partial class LX702_TextBoxWithoutAccessibleDescription : IXamlRule
{
    // Name/x:Name/Header/AutomationProperties.Name suppress on any non-empty value.
    // AutomationProperties.LabeledBy is value-dependent — see HasLabeledByEscape.
    private static readonly string[] SimplePresenceEscapeAttributes =
    {
        "Header",
        "AutomationProperties.Name",
    };

    private const string LabeledByAttribute = "AutomationProperties.LabeledBy";

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "TextBox") continue;
            if (HasNameEscape(element)) continue;
            if (HasSimplePresenceEscape(element)) continue;
            if (HasLabeledByEscape(element, context)) continue;

            var span = LocationHelpers.GetElementNameSpan(element);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "TextBox has no accessibility description; screen readers cannot announce its purpose. Set AutomationProperties.Name, Header, or AutomationProperties.LabeledBy=\"{x:Reference <label>}\".",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static bool HasNameEscape(XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            var isXName = attr.Name.LocalName == "Name" && XamlNamespaces.IsXamlNamespace(attr.Name.NamespaceName);
            if (isXName && !string.IsNullOrWhiteSpace(attr.Value)) return true;
        }
        var unprefixed = element.Attribute("Name");
        return unprefixed is not null && !string.IsNullOrWhiteSpace(unprefixed.Value);
    }

    private static bool HasSimplePresenceEscape(XElement element)
    {
        foreach (var name in SimplePresenceEscapeAttributes)
        {
            var attr = element.Attribute(name);
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value)) return true;
        }
        return false;
    }

    private static bool HasLabeledByEscape(XElement element, RuleContext context)
    {
        var labeledBy = element.Attribute(LabeledByAttribute);
        if (labeledBy is null) return false;
        var value = labeledBy.Value;
        if (string.IsNullOrWhiteSpace(value)) return false;

        if (!MarkupExtensionHelpers.IsMarkupExtension(value))
            return true; // non-extension literal — honour author intent

        if (!MarkupExtensionHelpers.TryParseExtension(value, out var info))
            return true; // malformed extension — don't second-guess

        if (!string.Equals(info.Name, "x:Reference", StringComparison.Ordinal)
            && !string.Equals(info.Name, "Reference", StringComparison.Ordinal))
            return true; // some other extension (Binding, StaticResource, etc.) — can't evaluate statically

        var targetName = ExtractReferenceTargetName(value);
        if (string.IsNullOrWhiteSpace(targetName)) return true; // empty/malformed reference — don't second-guess

        return context.Names.IsDefinedInScopeOf(element, targetName!);
    }

    private static string? ExtractReferenceTargetName(string value)
    {
        // {x:Reference Foo}, {x:Reference Name=Foo}, {Reference Foo}
        var trimmed = value.AsSpan().Trim();
        if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[^1] != '}') return null;
        var inner = trimmed[1..^1].Trim();

        // Skip the extension name (first token up to whitespace).
        var i = 0;
        while (i < inner.Length && !char.IsWhiteSpace(inner[i])) i++;
        if (i >= inner.Length) return null;
        var rest = inner[i..].Trim();
        if (rest.Length == 0) return null;

        // Named argument form (Name=Foo) — delegate to MarkupExtensionHelpers.
        if (rest[0] == '{' || rest.IndexOf('=') >= 0)
        {
            if (MarkupExtensionHelpers.TryParseExtension(value, out var info)
                && info.NamedArguments.TryGetValue("Name", out var named))
                return named;
            return null;
        }

        // Positional form: first token up to whitespace or comma.
        var end = 0;
        while (end < rest.Length && !char.IsWhiteSpace(rest[end]) && rest[end] != ',') end++;
        return rest[..end].ToString();
    }
}
