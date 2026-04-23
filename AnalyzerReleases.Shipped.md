; Shipped analyzer releases
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 0.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX001   | Tool     | Error    | Malformed XAML
LX002   | Tool     | Warning  | Unrecognized pragma directive
LX003   | Tool     | Error    | Malformed configuration
LX004   | Tool     | Error    | Cannot read file
LX005   | Tool     | Info     | Skipping non-XAML file
LX006   | Tool     | Error    | Internal error in rule

## Release 0.2.0

### New Rules

Rule ID | Category  | Severity | Notes
--------|-----------|----------|-------
LX200   | Bindings  | Info     | SelectedItem binding should be TwoWay
LX300   | Naming    | Warning  | x:Name should start with uppercase
LX400   | Resources | Info     | Hardcoded string; use a resource

## Release 0.3.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX100   | Layout   | Warning  | Grid.Row without matching RowDefinition
LX101   | Layout   | Warning  | Grid.Column without matching ColumnDefinition
LX102   | Layout   | Warning  | Grid.RowSpan exceeds available rows
LX103   | Layout   | Warning  | Grid.ColumnSpan exceeds available columns
LX104   | Layout   | Warning  | Grid definition shorthand not supported by target framework

## Release 0.4.0

### New Rules

Rule ID | Category   | Severity | Notes
--------|------------|----------|-------
LX201   | Bindings   | Info     | Prefer x:Bind over Binding
LX301   | Naming     | Warning  | x:Uid should start with uppercase
LX500   | Input      | Info     | TextBox lacks InputScope
LX501   | Input      | Warning  | Slider Minimum is greater than Maximum
LX502   | Input      | Warning  | Stepper Minimum is greater than Maximum
LX600   | Deprecated | Warning  | MediaElement is deprecated — use MediaPlayerElement

## Release 1.1.0

### New Rules

Rule ID | Category      | Severity | Notes
--------|---------------|----------|-------
LX402   | Resources     | Warning  | Image Source filename invalid on Android
LX503   | Input         | Info     | Entry lacks Keyboard
LX504   | Input         | Warning  | Password Entry lacks MaxLength
LX505   | Input         | Warning  | Pin lacks Label
LX506   | Input         | Info     | Slider sets both ThumbColor and ThumbImageSource
LX601   | Deprecated    | Info     | Line.Fill has no effect
LX700   | Accessibility | Info     | Image lacks accessibility description
LX701   | Accessibility | Info     | ImageButton lacks accessibility description
LX702   | Accessibility | Info     | TextBox lacks accessibility description
LX703   | Accessibility | Info     | Entry lacks accessibility description
LX800   | Platform      | Warning  | Uno platform XML namespace must be mc:Ignorable
