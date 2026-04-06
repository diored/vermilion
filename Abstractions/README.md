# DioRed.Vermilion.Abstractions

This package contains **public contracts** for the Vermilion platform:

- `IConnector` and related types (`MessagePostedEventArgs`, `PostResult`)
- `IChatStorage`
- Interaction content types (`IContent`, `TextContent`, `ImageUrlContent`, ...)
- Common domain primitives (`ChatId`, `ChatMetadata`, `UserRole`)

In `v15`, the contracts are cancellation-aware by default:

- async APIs use `CancellationToken ct = default`
- `IChatStorage.GetChatsAsync(...)` streams via `IAsyncEnumerable<ChatMetadata>`
- `ChatMetadata` is immutable and represents runtime chat state only
- `ChatMetadata` exposes convenience helpers such as `HasTag`, `WithTag`, and `WithoutTag`
- `IChatStorage` has convenience extensions such as `GetChatsArrayAsync()`
- storage migration can use `IChatStorageExport` and `ChatStorageMigrator`
- storage-only fields such as chat titles stay outside `ChatMetadata`

Use it when you want to implement a **custom connector** or **custom chat storage** without taking a dependency on the full Vermilion engine.

> If you just want to build a bot, install **DioRed.Vermilion.Hosting** (or **DioRed.Vermilion** metapackage) instead.
