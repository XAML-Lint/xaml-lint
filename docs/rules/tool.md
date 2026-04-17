# Tool / engine diagnostics (LX001–LX099)

Diagnostics in this range are emitted by the engine and CLI itself — not by lint rules analyzing your XAML. They report issues with the tool's inputs (malformed files, bad config, unreadable files) or with the tool's own execution (rule crashes).

| ID | Title | Default |
|---|---|---|
| [LX001](LX001.md) | Malformed XAML | error |
| [LX002](LX002.md) | Unrecognized pragma directive | warning |
| [LX003](LX003.md) | Malformed configuration | error |
| [LX004](LX004.md) | Cannot read file | error |
| [LX005](LX005.md) | Skipping non-XAML file | info |
| [LX006](LX006.md) | Internal error in rule | error |

These IDs are reserved — no content rule will ever take an ID in the LX0xx range.
