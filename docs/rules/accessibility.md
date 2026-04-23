# Accessibility (LX0700–LX0799)

Rules that check whether XAML is perceivable and operable by assistive technologies —
screen readers, keyboard navigation, and similar. These rules are frequently off by
default in `:recommended` because they flood typical codebases that haven't done a
deliberate a11y pass; they are on in `:strict` for teams that want to enforce
accessibility from the start. See each rule's doc for details.

| ID | Title | Default |
|---|---|---|
| [LX0700](LX0700.md) | Image lacks accessibility description | info (off in :recommended) |
| [LX0701](LX0701.md) | ImageButton lacks accessibility description | info (off in :recommended) |
| [LX0702](LX0702.md) | TextBox lacks accessibility description | info (off in :recommended) |
| [LX0703](LX0703.md) | Entry lacks accessibility description | info (off in :recommended) |
