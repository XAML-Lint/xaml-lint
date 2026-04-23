# Input / controls (LX0500–LX0599)

Rules that check input controls for missing hints (keyboard layout, IME behavior) and
semantically inconsistent attribute pairs (out-of-order `Minimum`/`Maximum`). These rules
complement the layout and binding checks — an input control with an impossible range
parses fine but fails silently at runtime.

| ID | Title | Default |
|---|---|---|
| [LX0500](LX0500.md) | TextBox lacks InputScope | info |
| [LX0501](LX0501.md) | Slider Minimum is greater than Maximum | warning |
| [LX0502](LX0502.md) | Stepper Minimum is greater than Maximum | warning |
| [LX0503](LX0503.md) | Entry lacks Keyboard | info |
| [LX0504](LX0504.md) | Password Entry lacks MaxLength | warning |
| [LX0505](LX0505.md) | Pin lacks Label | warning |
| [LX0506](LX0506.md) | Slider sets both ThumbColor and ThumbImageSource | info |
