using XamlLint.Core;

namespace XamlLint.DocTool;

public sealed record StubAction(string Path, bool Created, bool Deleted);

public static class DocStubWriter
{
    public const string StubSentinel = "<!-- generated stub; edit freely.";

    public static IReadOnlyList<StubAction> Run(string repoRoot, bool checkOnly)
    {
        var docsDir = Path.Combine(repoRoot, "docs", "rules");
        Directory.CreateDirectory(docsDir);

        var actions = new List<StubAction>();
        var catalogIds = GeneratedRuleCatalog.Rules.Select(r => r.Metadata).ToDictionary(m => m.Id);

        foreach (var meta in catalogIds.Values.OrderBy(m => m.Id, StringComparer.Ordinal))
        {
            var path = Path.Combine(docsDir, $"{meta.Id}.md");
            if (File.Exists(path)) continue;

            if (!checkOnly)
                File.WriteAllText(path, BuildStub(meta));
            actions.Add(new StubAction(path, Created: true, Deleted: false));
        }

        // Orphan detection: .md files in docs/rules that don't map to a catalog ID.
        foreach (var file in Directory.EnumerateFiles(docsDir, "LX*.md"))
        {
            var stem = Path.GetFileNameWithoutExtension(file);
            if (catalogIds.ContainsKey(stem)) continue;

            // Only delete stub-shaped files in non-check mode; require human review otherwise.
            var looksLikeStub = File.ReadAllText(file).Contains(StubSentinel);
            if (!looksLikeStub)
            {
                actions.Add(new StubAction(file, Created: false, Deleted: false));
                continue;
            }

            if (!checkOnly)
                File.Delete(file);
            actions.Add(new StubAction(file, Created: false, Deleted: true));
        }

        return actions;
    }

    private static string BuildStub(RuleMetadata m) => $$"""
# {{m.Id}}: {{m.Title}}

{{StubSentinel}} Upstream: {{m.UpstreamId ?? "—"}}. -->

## Cause

TBD — describe what triggers this rule.

## Rule description

TBD — longer-form explanation with inline-annotated XAML examples.

## How to fix violations

TBD — concrete steps the reader takes.

## How to suppress violations

- `<!-- xaml-lint disable once {{m.Id}} -->` (inline, one element)
- `<!-- xaml-lint disable {{m.Id}} -->` … `<!-- xaml-lint restore {{m.Id}} -->` (block)
- `xaml-lint.config.json` → `"rules": { "{{m.Id}}": "off" }` (file/project)
""";
}
