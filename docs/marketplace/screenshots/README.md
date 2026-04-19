# Screenshots for marketplace listing

Three PNGs expected before marketplace submission:

1. **`pretty-formatter.png`** — a terminal showing `xaml-lint lint src/**/*.xaml` output in `pretty` format. Goals: show colored headers, per-file grouping, aligned columns, and a concrete rule diagnostic with a short message.

2. **`claude-mid-edit.png`** — a Claude Code session showing the `PostToolUse` hook firing immediately after an `Edit` tool call on a `.xaml` file, with `xaml-lint` diagnostics reported back in the same turn. Goals: demonstrate the "Claude catches it as you write" value prop.

3. **`sarif-ci.png`** — GitHub Actions or Azure DevOps annotations panel showing `xaml-lint --format sarif` uploaded as a code-scanning result. Goals: show CI-facing use beyond the plugin surface.

Dimensions: capture at **1280×720** (16:9), PNG, each under 1 MB. If the marketplace submission flow later requires a different aspect ratio, re-capture and update.

If the submission flow requires a metadata sidecar (caption strings, alt text), record it alongside the PNGs as `captions.md` at submission time.
