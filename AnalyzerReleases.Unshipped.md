; Unshipped analyzer release
; Format: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.2.0

### New Rules

Rule ID | Category      | Severity | Notes
--------|---------------|----------|-------
LX0105   | Layout        | Warning  | Zero-sized RowDefinition / ColumnDefinition
LX0106   | Layout        | Warning  | Single-child Grid without row or column definitions
LX0202   | Bindings      | Warning  | Binding ElementName target does not exist
LX0203   | Bindings      | Warning  | x:Reference target does not exist
LX0704   | Accessibility | Info     | Icon button lacks accessibility description
