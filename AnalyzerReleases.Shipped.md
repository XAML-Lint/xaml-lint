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
