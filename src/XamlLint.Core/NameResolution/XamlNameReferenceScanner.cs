using System.Xml.Linq;

namespace XamlLint.Core.NameResolution;

/// <summary>
/// Same-file "uses" side of the XAML name-scope model. Given a document's root element
/// and a pre-built <see cref="XamlNameIndex"/>, walks every attribute in the document,
/// parses references via <see cref="ElementReference.TryParse"/> (markup-extension
/// forms) and a local predicate on attribute local names (literal-attribute forms),
/// and returns the set of declaration <see cref="XElement"/>s that are resolved by at
/// least one in-scope reference. Consumed by LX0302 to flag unused <c>x:Name</c>
/// declarations.
/// </summary>
/// <remarks>
/// Recognised reference forms:
/// <list type="bullet">
///   <item><c>{Binding ElementName=X}</c>, <c>{x:Reference X}</c>, <c>{x:Reference Name=X}</c>,
///         <c>{Reference X}</c> — any attribute value parsed by
///         <see cref="ElementReference.TryParse"/>.</item>
///   <item>Literal-value attributes whose local name is <c>TargetName</c> or <c>SourceName</c>,
///         or ends with <c>.TargetName</c> / <c>.SourceName</c> (attached-property syntax):
///         <c>Storyboard.TargetName</c>, <c>Setter.TargetName</c>, <c>Trigger.SourceName</c>,
///         <c>Condition.SourceName</c>, <c>EventTrigger.SourceName</c>, etc.</item>
/// </list>
/// Intentionally out of scope: <c>{x:Bind path.Name}</c> typed-path references, nested
/// markup extensions, and any reference originating from code-behind C#.
/// </remarks>
public static class XamlNameReferenceScanner
{
    public static IReadOnlySet<XElement> ComputeUsedDeclarations(XElement root, XamlNameIndex index)
    {
        var used = new HashSet<XElement>(ReferenceEqualityComparer.Instance);

        foreach (var element in root.DescendantsAndSelf())
        {
            foreach (var attr in element.Attributes())
            {
                // x:Name / Name declaration attributes flow through harmlessly: they are not
                // markup extensions and IsNameReferenceAttribute rejects the local name "Name".
                if (ElementReference.TryParse(attr.Value, out var info))
                {
                    var resolved = index.ResolveInScopeOf(element, info.TargetName);
                    if (resolved is not null) used.Add(resolved);
                    // An attribute value is either a markup extension or a plain literal — never
                    // both, so there is no point falling through to the literal-attribute branch.
                    continue;
                }

                if (!IsNameReferenceAttribute(attr.Name.LocalName)) continue;

                var value = attr.Value;
                if (string.IsNullOrWhiteSpace(value)) continue;

                var resolvedLiteral = index.ResolveInScopeOf(element, value);
                if (resolvedLiteral is not null) used.Add(resolvedLiteral);
            }
        }

        return used;
    }

    private static bool IsNameReferenceAttribute(string localName) =>
        localName == "TargetName" ||
        localName == "SourceName" ||
        localName.EndsWith(".TargetName", StringComparison.Ordinal) ||
        localName.EndsWith(".SourceName", StringComparison.Ordinal);
}
