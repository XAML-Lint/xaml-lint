# Rules

`xaml-lint`'s rule catalog, grouped by category. Each rule owns its own page (`LXNNNN.md`); each category page lists its rules with one-line titles and default severities.

| Category | ID range | What it covers |
|---|---|---|
| [Tool / engine diagnostics](tool.md) | LX0001–LX0099 | Engine and CLI errors — malformed input, bad config, rule crashes. Not XAML lints. |
| [Layout](layout.md) | LX0100–LX0199 | Grid attached-property checks (`Grid.Row`, `Grid.Column`, spans, definitions). |
| [Bindings / data](bindings.md) | LX0200–LX0299 | Binding markup-extension checks (`{Binding}`, `{x:Bind}`, `{x:Reference}`). |
| [Naming](naming.md) | LX0300–LX0399 | Identifier conventions for `x:Name`, `x:Uid`, `x:Key`. |
| [Resources / localization](resources.md) | LX0400–LX0499 | Hardcoded strings and resource-key idioms. |
| [Input / controls](input.md) | LX0500–LX0599 | Input control hints (keyboards, IME) and out-of-order `Min`/`Max` ranges. |
| [Usability](usability.md) | LX0600–LX0699 | Valid markup with degraded outcome — deprecated APIs, ineffective properties, broken nav surfaces. |
| [Accessibility](accessibility.md) | LX0700–LX0799 | Assistive-tech checks (screen readers, keyboard navigation). |
| [Platform](platform.md) | LX0800–LX0899 | Platform-integration wiring — namespace declarations, target-framework gating. |

Each rule is dialect-gated (WPF, WinUI 3, UWP, MAUI, Avalonia, Uno). See the individual rule pages for the supported dialect mask and `:recommended` / `:strict` defaults.
