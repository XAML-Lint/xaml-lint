using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX504",
    UpstreamId = "RXT301",
    Title = "Password Entry lacks MaxLength",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX504.md")]
public sealed partial class LX504_PasswordEntryWithoutMaxLength : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Entry") continue;

            var isPasswordAttr = element.Attribute("IsPassword");
            if (isPasswordAttr is null) continue;

            var value = isPasswordAttr.Value;
            if (MarkupExtensionHelpers.IsMarkupExtension(value)) continue;
            if (!string.Equals(value.Trim(), "True", StringComparison.OrdinalIgnoreCase)) continue;

            if (element.Attribute("MaxLength") is not null) continue;

            var span = LocationHelpers.GetAttributeSpan(isPasswordAttr, context.Source);
            yield return new Diagnostic(
                RuleId: Metadata.Id,
                Severity: Metadata.DefaultSeverity,
                Message: "Password Entry should set MaxLength to cap input length (UX + security best practice).",
                File: document.FilePath,
                StartLine: span.StartLine,
                StartCol: span.StartCol,
                EndLine: span.EndLine,
                EndCol: span.EndCol,
                HelpUri: Metadata.HelpUri);
        }
    }
}
