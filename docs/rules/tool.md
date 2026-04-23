# Tool / engine diagnostics (LX0001–LX0099)

Diagnostics in this range are emitted by the engine and CLI itself — not by lint rules analyzing your XAML. They report issues with the tool's inputs (malformed files, bad config, unreadable files) or with the tool's own execution (rule crashes).

| ID | Title | Default |
|---|---|---|
| [LX0001](LX0001.md) | Malformed XAML | error |
| [LX0002](LX0002.md) | Unrecognized pragma directive | warning |
| [LX0003](LX0003.md) | Malformed configuration | error |
| [LX0004](LX0004.md) | Cannot read file | error |
| [LX0005](LX0005.md) | Skipping non-XAML file | info |
| [LX0006](LX0006.md) | Internal error in rule | error |

These IDs are reserved — no content rule will ever take an ID in the LX0xx range.
