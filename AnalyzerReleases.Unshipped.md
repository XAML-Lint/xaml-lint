; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
LX100   | Layout   | Warning  | Grid.Row without matching RowDefinition
LX101   | Layout   | Warning  | Grid.Column without matching ColumnDefinition
LX102   | Layout   | Warning  | Grid.RowSpan exceeds available rows
LX103   | Layout   | Warning  | Grid.ColumnSpan exceeds available columns
LX104   | Layout   | Warning  | Grid definition shorthand not supported by target framework
