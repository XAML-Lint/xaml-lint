# Deprecated patterns (LX0600–LX0699)

Rules that flag XAML elements and attributes that were once idiomatic but have been
superseded by better replacements. Each rule points at the modern equivalent so the fix is
mechanical. The range is dialect-scoped: a pattern deprecated on UWP/WinUI 3 may still be
the primary API on WPF.

This category covers two related kinds of problem: (a) APIs that have been
superseded by a newer replacement (the original intent of the category — see
LX0600), and (b) usages that are still syntactically valid but have no runtime
effect, and therefore represent dead or misleading markup. Both produce XAML
that should be rewritten for clarity, not for correctness.

| ID | Title | Default |
|---|---|---|
| [LX0600](LX0600.md) | MediaElement is deprecated — use MediaPlayerElement | warning |
| [LX0601](LX0601.md) | Line.Fill has no effect | info |
