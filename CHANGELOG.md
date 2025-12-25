# Changelog

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
