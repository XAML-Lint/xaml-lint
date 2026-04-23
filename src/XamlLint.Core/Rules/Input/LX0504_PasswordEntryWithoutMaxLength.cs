using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.Rules.Input;

[XamlRule(
    Id = "LX0504",
    UpstreamId = "RXT301",
    Title = "Password Entry lacks MaxLength",
    DefaultSeverity = Severity.Warning,
    Dialects = Dialect.Maui,
    HelpUri = "https://github.com/XAML-Lint/xaml-lint/blob/main/docs/rules/LX0504.md")]
public sealed partial class LX0504_PasswordEntryWithoutMaxLength : IXamlRule
{
    public IEnumerable<Diagnostic> Analyze(XamlDocument document, RuleContext context)
    {
        if (document.Root is null) yield break;

        foreach (var element in document.Root.DescendantsAndSelf())
        {
            if (element.Name.LocalName != "Entry") continue;

            var isPassword = PropertyElementHelpers.TryGetValueAndSource(element, "IsPassword");
            if (isPassword is null) continue;

            var value = isPassword.Value.Value;
            if (MarkupExtensionHelpers.IsMarkupExtension(value)) continue;
            if (!string.Equals(value.Trim(), "True", StringComparison.OrdinalIgnoreCase)) continue;

            if (PropertyElementHelpers.HasAttributeOrPropertyElement(element, "MaxLength")) continue;

            var span = isPassword.Value.Source switch
            {
                XAttribute attr => LocationHelpers.GetAttributeSpan(attr, context.Source),
                XElement elem => LocationHelpers.GetElementNameSpan(elem),
                _ => throw new InvalidOperationException(
                    "TryGetValueAndSource must return an XAttribute or XElement source."),
            };

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
