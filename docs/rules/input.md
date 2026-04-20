# Input / controls (LX500–LX599)

Rules that check input controls for missing hints (keyboard layout, IME behavior) and
semantically inconsistent attribute pairs (out-of-order `Minimum`/`Maximum`). These rules
complement the layout and binding checks — an input control with an impossible range
parses fine but fails silently at runtime.

| ID | Title | Default |
|---|---|---|
| [LX500](LX500.md) | TextBox lacks InputScope | info |
| [LX501](LX501.md) | Slider Minimum is greater than Maximum | warning |
| [LX502](LX502.md) | Stepper Minimum is greater than Maximum | warning |
| [LX503](LX503.md) | Entry lacks Keyboard | info |
| [LX504](LX504.md) | Password Entry lacks MaxLength | warning |
