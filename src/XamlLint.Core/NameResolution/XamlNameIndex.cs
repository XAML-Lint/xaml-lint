using System.Xml.Linq;
using XamlLint.Core.Helpers;

namespace XamlLint.Core.NameResolution;

/// <summary>
/// A per-document index of named elements, organised into XAML namescope-aware scopes.
/// Rules use this to validate <c>{x:Reference Foo}</c> targets — a reference is valid iff
/// the name is declared in the innermost scope enclosing the reference site.
/// </summary>
/// <remarks>
/// XAML namescope semantics:
/// <list type="bullet">
///   <item>Root scope: the document.</item>
///   <item>Each of <c>ControlTemplate</c>, <c>DataTemplate</c>, <c>ItemsPanelTemplate</c>, and
///         <c>HierarchicalDataTemplate</c> opens a new scope — names inside are isolated from
///         the outer scope, and the outer scope is not visible from inside.</item>
///   <item>Both unprefixed <c>Name="…"</c> and <c>x:Name="…"</c> (in the XAML 2006 or 2009
///         namespace) register into the current scope.</item>
///   <item>Empty/whitespace names are ignored. Names are case-sensitive.</item>
///   <item>Duplicate names in the same scope are tolerated; the first one wins.</item>
/// </list>
/// </remarks>
public sealed class XamlNameIndex
{
    private static readonly HashSet<string> ScopeOpenerLocalNames = new(StringComparer.Ordinal)
    {
        "ControlTemplate",
        "DataTemplate",
        "ItemsPanelTemplate",
        "HierarchicalDataTemplate",
    };

    private readonly Scope _root;
    private readonly Dictionary<XElement, Scope> _elementToScope;

    private XamlNameIndex(Scope root, Dictionary<XElement, Scope> elementToScope)
    {
        _root = root;
        _elementToScope = elementToScope;
    }

    public static XamlNameIndex Build(XElement root)
    {
        var rootScope = new Scope(scopeOwner: root, parent: null);
        var elementToScope = new Dictionary<XElement, Scope>(ReferenceEqualityComparer.Instance);
        WalkInto(root, rootScope, elementToScope);
        return new XamlNameIndex(rootScope, elementToScope);
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="name"/> is declared in the innermost scope
    /// enclosing <paramref name="referenceElement"/>. A reference element not owned by the
    /// document tree returns <c>false</c>.
    /// </summary>
    public bool IsDefinedInScopeOf(XElement referenceElement, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (!_elementToScope.TryGetValue(referenceElement, out var scope)) return false;
        return scope.Names.Contains(name);
    }

    /// <summary>
    /// Enumerates every (name, declaring-element) pair across all scopes.
    /// </summary>
    public IEnumerable<(string Name, XElement DeclaringElement)> AllNames()
    {
        foreach (var pair in EnumerateScopes(_root))
        foreach (var entry in pair.Declarations)
            yield return (entry.Key, entry.Value);
    }

    private static IEnumerable<Scope> EnumerateScopes(Scope root)
    {
        var stack = new Stack<Scope>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var s = stack.Pop();
            yield return s;
            foreach (var child in s.Children) stack.Push(child);
        }
    }

    private static void WalkInto(XElement element, Scope currentScope, Dictionary<XElement, Scope> map)
    {
        map[element] = currentScope;

        var nameValue = TryReadName(element);
        if (nameValue is not null && !currentScope.Names.Contains(nameValue))
        {
            currentScope.Names.Add(nameValue);
            currentScope.Declarations[nameValue] = element;
        }

        foreach (var child in element.Elements())
        {
            var nextScope = ScopeOpenerLocalNames.Contains(child.Name.LocalName)
                ? currentScope.OpenChild(child)
                : currentScope;

            WalkInto(child, nextScope, map);
        }
    }

    private static string? TryReadName(XElement element)
    {
        // Prefer x:Name (XAML 2006 or 2009) when both are present.
        foreach (var attr in element.Attributes())
        {
            var ns = attr.Name.NamespaceName;
            var isXName = attr.Name.LocalName == "Name" && XamlNamespaces.IsXamlNamespace(ns);
            if (isXName && !string.IsNullOrWhiteSpace(attr.Value))
                return attr.Value;
        }

        var unprefixed = element.Attribute("Name");
        if (unprefixed is not null && !string.IsNullOrWhiteSpace(unprefixed.Value))
            return unprefixed.Value;

        return null;
    }

    private sealed class Scope
    {
        public Scope(XElement scopeOwner, Scope? parent)
        {
            ScopeOwner = scopeOwner;
            Parent = parent;
            Children = new List<Scope>();
            Names = new HashSet<string>(StringComparer.Ordinal);
            Declarations = new Dictionary<string, XElement>(StringComparer.Ordinal);
        }

        public XElement ScopeOwner { get; }
        public Scope? Parent { get; }
        public List<Scope> Children { get; }
        public HashSet<string> Names { get; }
        public Dictionary<string, XElement> Declarations { get; }

        public Scope OpenChild(XElement scopeOwner)
        {
            var child = new Scope(scopeOwner, this);
            Children.Add(child);
            return child;
        }
    }
}
