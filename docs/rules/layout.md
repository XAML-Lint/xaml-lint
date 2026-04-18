# Layout (LX100–LX199)

Rules that check layout containers — Grid, StackPanel, DockPanel, Canvas, and similar —
for attached-property values that disagree with the container's declared shape. Today's
coverage is Grid-specific (`Grid.Row`, `Grid.Column`, `Grid.RowSpan`, `Grid.ColumnSpan`);
StackPanel and DockPanel rules will be added as real-world false negatives surface.

| ID | Title | Default |
|---|---|---|
| [LX100](LX100.md) | Grid.Row without matching RowDefinition | warning |
| [LX101](LX101.md) | Grid.Column without matching ColumnDefinition | warning |
| [LX102](LX102.md) | Grid.RowSpan exceeds available rows | warning |
| [LX103](LX103.md) | Grid.ColumnSpan exceeds available columns | warning |
