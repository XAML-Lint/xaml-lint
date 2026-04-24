# Bindings / data (LX0200–LX0299)

Rules that inspect data-binding expressions (`{Binding …}`, `{x:Bind …}`, `{TemplateBinding …}`).
These rules fire on attributes whose values are XAML markup extensions and examine the
extension's arguments — they do not run type analysis or verify data-context paths.

| ID | Title | Default |
|---|---|---|
| [LX0200](LX0200.md) | SelectedItem binding should be TwoWay | info |
| [LX0201](LX0201.md) | Prefer x:Bind over Binding | info |
| [LX0202](LX0202.md) | Binding ElementName target does not exist | warning |
| [LX0203](LX0203.md) | x:Reference target does not exist | warning |
