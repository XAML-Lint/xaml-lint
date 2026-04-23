using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Platform;

[XamlRule(
    Id = "LX0800",
    UpstreamId = "RXT700",
    Title = "Uno platform XML namespace must be mc:Ignorable",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.All,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0800.md")]
public sealed partial class LX0800_UnoPlatformXmlnsNotIgnorable : IXamlRule
{
    // mc:Ignorable lives in the Markup Compatibility namespace; look it up by expanded name
    // rather than by prefix, since authors are free to choose any prefix.
    private static readonly XName IgnorableAttributeName =
        XName.Get("Ignorable", XamlNamespaces.MarkupCompatibility);

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        var root = document.Root;
        if (root is null) yield break;

        var ignorablePrefixes = ReadIgnorablePrefixes(root);

        foreach (var attr in root.Attributes())
        {
            if (!IsXmlnsPrefixDeclaration(attr, out var prefix)) continue;
            if (!XamlNamespaces.UnoPlatformUris.TryGetValue(attr.Value, out var platformName)) continue;
            if (ignorablePrefixes.Contains(prefix)) continue;

            var span = LocationHelpers.GetAttributeSpan(attr, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"XML namespace prefix '{prefix}' targets Uno platform '{platformName}' and must be listed in mc:Ignorable to prevent load-time failures on non-Uno platforms.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static HashSet<string> ReadIgnorablePrefixes(XElement root)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        var attr = root.Attribute(IgnorableAttributeName);
        if (attr is null) return set;

        foreach (var token in attr.Value.Split(
            new[] { ' ', '\t', '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries))
        {
            set.Add(token);
        }
        return set;
    }

    /// <summary>
    /// Returns true when <paramref name="attr"/> is a prefixed <c>xmlns:</c> declaration,
    /// writing the prefix into <paramref name="prefix"/>. Skips the default <c>xmlns</c>.
    /// </summary>
    private static bool IsXmlnsPrefixDeclaration(XAttribute attr, out string prefix)
    {
        if (!attr.IsNamespaceDeclaration)
        {
            prefix = string.Empty;
            return false;
        }
        // For xmlns:foo LocalName is "foo"; for the default xmlns LocalName is "xmlns".
        if (attr.Name.LocalName == "xmlns")
        {
            prefix = string.Empty;
            return false;
        }
        prefix = attr.Name.LocalName;
        return true;
    }
}
