; Shipped analyzer releases
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
;
; 2026-04-23: rule IDs renumbered from LX### to LX####. Historical
; release entries were rewritten to the new scheme — this file
; represents the current rule catalog, not a bit-for-bit record of
; what shipped under the old IDs. Pre-adoption breaking change
; per docs/roadmap.md.

## Release 0.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX0001   | Tool     | Error    | Malformed XAML
LX0002   | Tool     | Warning  | Unrecognized pragma directive
LX0003   | Tool     | Error    | Malformed configuration
LX0004   | Tool     | Error    | Cannot read file
LX0005   | Tool     | Info     | Skipping non-XAML file
LX0006   | Tool     | Error    | Internal error in rule

## Release 0.2.0

### New Rules

Rule ID | Category  | Severity | Notes
--------|-----------|----------|-------
LX0200   | Bindings  | Info     | SelectedItem binding should be TwoWay
LX0300   | Naming    | Warning  | x:Name should start with uppercase
LX0400   | Resources | Info     | Hardcoded string; use a resource

## Release 0.3.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX0100   | Layout   | Warning  | Grid.Row without matching RowDefinition
LX0101   | Layout   | Warning  | Grid.Column without matching ColumnDefinition
LX0102   | Layout   | Warning  | Grid.RowSpan exceeds available rows
LX0103   | Layout   | Warning  | Grid.ColumnSpan exceeds available columns
LX0104   | Layout   | Warning  | Grid definition shorthand not supported by target framework

## Release 0.4.0

### New Rules

Rule ID | Category   | Severity | Notes
--------|------------|----------|-------
LX0201   | Bindings   | Info     | Prefer x:Bind over Binding
LX0301   | Naming     | Warning  | x:Uid should start with uppercase
LX0500   | Input      | Info     | TextBox lacks InputScope
LX0501   | Input      | Warning  | Slider Minimum is greater than Maximum
LX0502   | Input      | Warning  | Stepper Minimum is greater than Maximum
LX0600   | Usability  | Warning  | MediaElement is deprecated — use MediaPlayerElement

## Release 1.1.0

### New Rules

Rule ID | Category      | Severity | Notes
--------|---------------|----------|-------
LX0402   | Resources     | Warning  | Image Source filename invalid on Android
LX0503   | Input         | Info     | Entry lacks Keyboard
LX0504   | Input         | Warning  | Password Entry lacks MaxLength
LX0505   | Input         | Warning  | Pin lacks Label
LX0506   | Input         | Info     | Slider sets both ThumbColor and ThumbImageSource
LX0601   | Usability     | Info     | Line.Fill has no effect
LX0700   | Accessibility | Info     | Image lacks accessibility description
LX0701   | Accessibility | Info     | ImageButton lacks accessibility description
LX0702   | Accessibility | Info     | TextBox lacks accessibility description
LX0703   | Accessibility | Info     | Entry lacks accessibility description
LX0800   | Platform      | Warning  | Uno platform XML namespace must be mc:Ignorable
