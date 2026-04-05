# Changelog

## 15.0.0

See also: [MIGRATION.md](./MIGRATION.md)

### Added
- **Chat storages**:
  - new `DioRed.Vermilion.ChatStorage.Sqlite` package
  - new `DioRed.Vermilion.ChatStorage.MongoDb` package
- **Examples**:
  - `examples/QuickStart.Telegram.Sqlite`
  - `examples/QuickStart.Telegram.MongoDb`
- **Storage migrations**:
  - versioned runtime schema management for SQL Server
  - built-in compatibility/migration flow for Azure Table keys
  - schema/version handling for SQLite and MongoDB providers
- **Typed storage exceptions**:
  - `ChatAlreadyExistsException`
  - `ChatNotFoundException`
  - `StorageMigrationException`
- **Test infrastructure**:
  - TUnit-based storage test projects
  - Docker/Testcontainers integration tests for SQL Server, Azurite, and MongoDB
  - migration coverage for legacy storage formats
- **Documentation**:
  - `MIGRATION.md` for upgrading from `v14` to `v15`
  - `LICENSE` file with the MIT license text

### Fixed
- **Package metadata**:
  - NuGet tags are now package-specific instead of copied across unrelated packages
  - package metadata now includes repository/project links and stable release-notes pointers
- **Storage identity**:
  - all supported persistent chat storages now treat `ChatId` as `ConnectorKey + Type + Id`
  - SQL Server and Azure Table no longer alias chats that share numeric ids across chat types
- **InMemory chat storage**: duplicate chats are now detected by `ChatId`, not by object reference
- **Telegram connector**: restart after `StopAsync()` works correctly
- **BotCore inbound processing**:
  - replaced fire-and-forget message handling with a bounded queue
  - shutdown now coordinates message pump completion more safely
- **Azurite/SQL integration coverage**: legacy migration paths are now exercised automatically in tests

### Breaking changes
- `IChatStorage` was redesigned:
  - all async methods now use `CancellationToken ct = default`
  - `GetChatsAsync(...)` now returns `IAsyncEnumerable<ChatMetadata>`
  - `AddChatAsync(...)` is now a single overload with optional `title`
- `ChatMetadata` is now immutable
- chat titles are treated as storage-only data and are not part of runtime `ChatMetadata`
- `IConnector.PostAsync(...)` now accepts `CancellationToken ct = default`
- `ICommandHandler.HandleAsync(...)` now accepts `CancellationToken ct = default`
- storage providers now throw typed Vermilion exceptions instead of relying on generic `ArgumentException` / `InvalidOperationException` for common storage cases
- hosting helpers were updated to the new cancellation-aware handler model
- scheduled jobs now use `IScheduledJob.HandleAsync(...)` as the primary async contract
- visibility configuration was renamed from `ClientsPolicy` to `Visibility` / `BotVisibility`, with hosting APIs moving toward `ConfigureVisibility(...)`, `Public()`, and `PrivateTo(...)`

## 14.0.0

### Added
- **Abstractions**: new `DioRed.Vermilion.Abstractions` package with public contracts (connector/storage interfaces and content primitives).
- **JsonFile chat storage**: new `DioRed.Vermilion.ChatStorage.JsonFile` package (single JSON file, atomic writes).
- **Hosting**: `IHostBuilder.AddVermilion(...)` + `VermilionBuilder`.
  - The configuration delegate runs during `ConfigureServices`, so plugins can register DI services.
  - Vermilion configuration is collected and applied later, after the service provider is built.
- **Action-based convenience overloads**
  - `ChatStorageCollection.UseAzureTable(AzureStorageSettings settings, Action<AzureTableChatStorageOptions>? configure = null)`
  - `ChatStorageCollection.UseSqlServer(string connectionString, Action<SqlServerChatStorageOptions>? configure = null)`
  - `ConnectorsCollection.AddTelegram(string botToken, Action<TelegramConnectorOptions>? configure = null)`
- **Documentation & samples**
  - Root README and per-package READMEs (used as NuGet package readmes).
  - `examples/` folder with ready-to-run sample hosts.
  - New sample: `examples/QuickStart.Telegram.JsonFile`.

### Fixed
- **Hosting**: `BotCoreBuilder` reads `BotOptions` from the `Vermilion` configuration section correctly.
- **Hosting**: `ClientPolicyBuilder` no longer builds a self-referencing policy delegate (stack overflow fix).
- **Core**:
  - Bot initialization no longer blocks with `GetAwaiter().GetResult()`.
  - BotCore synchronization is instance-scoped (multiple bots in one process are now possible).
- **Telegram connector**:
  - Retry logic now retries correctly (retries no longer re-await the same failed `Task`).
  - Bot info is resolved asynchronously in `StartAsync` (no sync-over-async in the constructor).
  - Safer socket-exception detection.
  - A bytes-to-photo send path disposes streams properly.
- **SQL Server chat storage**:
  - Table creation includes the `[Tags]` column.
  - Backward-compatible migration: if the table exists but lacks `[Tags]`, it is added.
  - `GetChatAsync` uses `QueryFirstOrDefaultAsync` and throws a friendly `ArgumentException` when a chat is missing.
- **Azure Table chat storage**: `CreateIfNotExistsAsync` is cached and reused across calls (including update/remove).

### Breaking changes
- `BotCoreBuilder.ConfigureChatStorage(...)` now throws `InvalidOperationException` if called more than once.
- `DioRed.Vermilion.Core` forwards connector/storage contract types to `DioRed.Vermilion.Abstractions` (binary compatible via type forwarding, but reflection/assembly-based assumptions may change).
- `DioRed.Vermilion.Handling.ClientsPolicy` enum renamed to `CommandClientsPolicy` to avoid name collision with runtime `ClientsPolicy`.
