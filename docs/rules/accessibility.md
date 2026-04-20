# Accessibility (LX700–LX799)

Rules that check whether XAML is perceivable and operable by assistive technologies —
screen readers, keyboard navigation, and similar. These rules are frequently off by
default in `:recommended` because they flood typical codebases that haven't done a
deliberate a11y pass; they are on in `:strict` for teams that want to enforce
accessibility from the start. See each rule's doc for details.

| ID | Title | Default |
|---|---|---|
| [LX700](LX700.md) | Image lacks accessibility description | info (off in :recommended) |
| [LX701](LX701.md) | ImageButton lacks accessibility description | info (off in :recommended) |
| [LX702](LX702.md) | TextBox lacks accessibility description | info (off in :recommended) |
