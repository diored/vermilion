# DioRed.Vermilion.Core

Core runtime of the Vermilion chat-bot engine.

This package contains:
- message processing pipeline
- command handling primitives
- core runtime logic (contracts are in `DioRed.Vermilion.Abstractions`)

It intentionally **does not** include any connector (Telegram, etc.) or storage implementation.

Most applications will use `DioRed.Vermilion.Hosting` (Generic Host integration) plus one connector and one storage package.
