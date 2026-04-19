---
description: Lint one or more XAML files with xaml-lint. Accepts a path or glob.
argument-hint: "<path-or-glob>"
---

Run `xaml-lint lint $ARGUMENTS` and report the diagnostics back to the user. If the tool emits an empty results array, say "No XAML issues found." If there are diagnostics, summarize them grouped by file, with rule ID, message, and line number.

See the `lint-xaml` skill for full guidance on output shape, severity handling, and suppressions.
