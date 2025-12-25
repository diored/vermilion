# DioRed.Vermilion.Abstractions

This package contains **public contracts** for the Vermilion platform:

- `IConnector` and related types (`MessagePostedEventArgs`, `PostResult`)
- `IChatStorage`
- Interaction content types (`IContent`, `TextContent`, `ImageUrlContent`, ...)
- Common domain primitives (`ChatId`, `ChatMetadata`, `UserRole`)

Use it when you want to implement a **custom connector** or **custom chat storage** without taking a dependency on the full Vermilion engine.

> If you just want to build a bot, install **DioRed.Vermilion.Hosting** (or **DioRed.Vermilion** metapackage) instead.
