using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Resources;

[XamlRule(
    Id = "LX402",
    UpstreamId = "RXT310",
    Title = "Image Source filename invalid on Android",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX402.md")]
public sealed partial class LX402_ImageSourceInvalidAndroidFilename : IXamlRule
{
    private static readonly string[] UriSchemes =
        { "http://", "https://", "ms-appx:", "ms-appdata:", "file://" };

    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Image") continue;

            var sourceAttr = element.Attribute("Source");
            if (sourceAttr is null) continue;

            var value = sourceAttr.Value;
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (MarkupExtensionHelpers.IsMarkupExtension(value)) continue;
            if (HasUriScheme(value)) continue;

            var leaf = ExtractLeafName(value);
            if (leaf.Length == 0) continue;
            if (IsValidAndroidDrawableName(leaf)) continue;

            var span = LocationHelpers.GetAttributeSpan(sourceAttr, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: $"Image Source filename '{leaf}' violates Android drawable naming (lowercase letters, digits, underscore, and period only; cannot start with a digit). The image will fail to bundle on Android.",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }

    private static bool HasUriScheme(string value)
    {
        foreach (var scheme in UriSchemes)
        {
            if (value.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static string ExtractLeafName(string path)
    {
        var lastSep = path.LastIndexOfAny(new[] { '/', '\\' });
        return lastSep < 0 ? path : path.Substring(lastSep + 1);
    }

    private static bool IsValidAndroidDrawableName(string leaf)
    {
        if (leaf.Length == 0) return false;
        if (char.IsDigit(leaf[0])) return false;
        foreach (var c in leaf)
        {
            var ok = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '.';
            if (!ok) return false;
        }
        return true;
    }
}
