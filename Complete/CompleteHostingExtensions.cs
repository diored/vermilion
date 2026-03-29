using DioRed.Common.AzureStorage;
using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors.Telegram;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Builder-level convenience methods available from the complete Vermilion package.
/// </summary>
public static class CompleteHostingExtensions
{
    extension(VermilionBuilder builder)
    {
        /// <summary>
        /// Uses the in-memory chat storage implementation.
        /// </summary>
        public VermilionBuilder UseInMemoryChatStorage()
            => builder.ConfigureChatStorage(c => c.UseInMemory());

        /// <summary>
        /// Uses JSON file storage configured from the application configuration.
        /// </summary>
        public VermilionBuilder UseJsonFileChatStorage()
            => builder.ConfigureChatStorage(c => c.UseJsonFile());

        /// <summary>
        /// Uses JSON file storage with the specified options.
        /// </summary>
        public VermilionBuilder UseJsonFileChatStorage(JsonFileChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseJsonFile(options));

        /// <summary>
        /// Uses JSON file storage with the specified file path and optional configuration.
        /// </summary>
        public VermilionBuilder UseJsonFileChatStorage(
            string filePath,
            Action<JsonFileChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseJsonFile(filePath, configure));

        /// <summary>
        /// Uses Azure Table storage configured from the application configuration.
        /// </summary>
        public VermilionBuilder UseAzureTableChatStorage()
            => builder.ConfigureChatStorage(c => c.UseAzureTable());

        /// <summary>
        /// Uses Azure Table storage with the specified options.
        /// </summary>
        public VermilionBuilder UseAzureTableChatStorage(AzureTableChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseAzureTable(options));

        /// <summary>
        /// Uses Azure Table storage with the specified Azure Storage settings.
        /// </summary>
        public VermilionBuilder UseAzureTableChatStorage(
            AzureStorageSettings settings,
            Action<AzureTableChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseAzureTable(settings, configure));

        /// <summary>
        /// Uses SQL Server storage configured from the application configuration.
        /// </summary>
        public VermilionBuilder UseSqlServerChatStorage()
            => builder.ConfigureChatStorage(c => c.UseSqlServer());

        /// <summary>
        /// Uses SQL Server storage with the specified options.
        /// </summary>
        public VermilionBuilder UseSqlServerChatStorage(SqlServerChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseSqlServer(options));

        /// <summary>
        /// Uses SQL Server storage with the specified connection string and optional configuration.
        /// </summary>
        public VermilionBuilder UseSqlServerChatStorage(
            string connectionString,
            Action<SqlServerChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseSqlServer(connectionString, configure));

        /// <summary>
        /// Uses SQLite storage configured from the application configuration.
        /// </summary>
        public VermilionBuilder UseSqliteChatStorage()
            => builder.ConfigureChatStorage(c => c.UseSqlite());

        /// <summary>
        /// Uses SQLite storage with the specified options.
        /// </summary>
        public VermilionBuilder UseSqliteChatStorage(SqliteChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseSqlite(options));

        /// <summary>
        /// Uses SQLite storage with the specified connection string and optional configuration.
        /// </summary>
        public VermilionBuilder UseSqliteChatStorage(
            string connectionString,
            Action<SqliteChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseSqlite(connectionString, configure));

        /// <summary>
        /// Uses MongoDB storage configured from the application configuration.
        /// </summary>
        public VermilionBuilder UseMongoDbChatStorage()
            => builder.ConfigureChatStorage(c => c.UseMongoDb());

        /// <summary>
        /// Uses MongoDB storage with the specified options.
        /// </summary>
        public VermilionBuilder UseMongoDbChatStorage(MongoDbChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseMongoDb(options));

        /// <summary>
        /// Uses MongoDB storage with the specified connection string and optional configuration.
        /// </summary>
        public VermilionBuilder UseMongoDbChatStorage(
            string connectionString,
            Action<MongoDbChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseMongoDb(connectionString, configure));

        /// <summary>
        /// Uses Telegram connectors configured from the application configuration.
        /// </summary>
        public VermilionBuilder UseTelegram()
            => builder.ConfigureConnectors(c => c.AddTelegram());

        /// <summary>
        /// Uses Telegram with the specified options.
        /// </summary>
        public VermilionBuilder UseTelegram(TelegramConnectorOptions options)
            => builder.ConfigureConnectors(c => c.AddTelegram(options));

        /// <summary>
        /// Uses Telegram with the specified bot token and optional configurator.
        /// </summary>
        public VermilionBuilder UseTelegram(
            string botToken,
            Action<TelegramConnectorOptions>? configure = null
        )
            => builder.ConfigureConnectors(c => c.AddTelegram(botToken, configure));
    }

    extension(BotCoreBuilder builder)
    {
        /// <summary>
        /// Uses the in-memory chat storage implementation.
        /// </summary>
        public BotCoreBuilder UseInMemoryChatStorage()
            => builder.ConfigureChatStorage(c => c.UseInMemory());

        /// <summary>
        /// Uses JSON file storage configured from the application configuration.
        /// </summary>
        public BotCoreBuilder UseJsonFileChatStorage()
            => builder.ConfigureChatStorage(c => c.UseJsonFile());

        /// <summary>
        /// Uses JSON file storage with the specified options.
        /// </summary>
        public BotCoreBuilder UseJsonFileChatStorage(JsonFileChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseJsonFile(options));

        /// <summary>
        /// Uses JSON file storage with the specified file path and optional configuration.
        /// </summary>
        public BotCoreBuilder UseJsonFileChatStorage(
            string filePath,
            Action<JsonFileChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseJsonFile(filePath, configure));

        /// <summary>
        /// Uses Azure Table storage configured from the application configuration.
        /// </summary>
        public BotCoreBuilder UseAzureTableChatStorage()
            => builder.ConfigureChatStorage(c => c.UseAzureTable());

        /// <summary>
        /// Uses Azure Table storage with the specified options.
        /// </summary>
        public BotCoreBuilder UseAzureTableChatStorage(AzureTableChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseAzureTable(options));

        /// <summary>
        /// Uses Azure Table storage with the specified Azure Storage settings.
        /// </summary>
        public BotCoreBuilder UseAzureTableChatStorage(
            AzureStorageSettings settings,
            Action<AzureTableChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseAzureTable(settings, configure));

        /// <summary>
        /// Uses SQL Server storage configured from the application configuration.
        /// </summary>
        public BotCoreBuilder UseSqlServerChatStorage()
            => builder.ConfigureChatStorage(c => c.UseSqlServer());

        /// <summary>
        /// Uses SQL Server storage with the specified options.
        /// </summary>
        public BotCoreBuilder UseSqlServerChatStorage(SqlServerChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseSqlServer(options));

        /// <summary>
        /// Uses SQL Server storage with the specified connection string and optional configuration.
        /// </summary>
        public BotCoreBuilder UseSqlServerChatStorage(
            string connectionString,
            Action<SqlServerChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseSqlServer(connectionString, configure));

        /// <summary>
        /// Uses SQLite storage configured from the application configuration.
        /// </summary>
        public BotCoreBuilder UseSqliteChatStorage()
            => builder.ConfigureChatStorage(c => c.UseSqlite());

        /// <summary>
        /// Uses SQLite storage with the specified options.
        /// </summary>
        public BotCoreBuilder UseSqliteChatStorage(SqliteChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseSqlite(options));

        /// <summary>
        /// Uses SQLite storage with the specified connection string and optional configuration.
        /// </summary>
        public BotCoreBuilder UseSqliteChatStorage(
            string connectionString,
            Action<SqliteChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseSqlite(connectionString, configure));

        /// <summary>
        /// Uses MongoDB storage configured from the application configuration.
        /// </summary>
        public BotCoreBuilder UseMongoDbChatStorage()
            => builder.ConfigureChatStorage(c => c.UseMongoDb());

        /// <summary>
        /// Uses MongoDB storage with the specified options.
        /// </summary>
        public BotCoreBuilder UseMongoDbChatStorage(MongoDbChatStorageOptions options)
            => builder.ConfigureChatStorage(c => c.UseMongoDb(options));

        /// <summary>
        /// Uses MongoDB storage with the specified connection string and optional configuration.
        /// </summary>
        public BotCoreBuilder UseMongoDbChatStorage(
            string connectionString,
            Action<MongoDbChatStorageOptions>? configure = null
        )
            => builder.ConfigureChatStorage(c => c.UseMongoDb(connectionString, configure));

        /// <summary>
        /// Uses Telegram connectors configured from the application configuration.
        /// </summary>
        public BotCoreBuilder UseTelegram()
            => builder.ConfigureConnectors(c => c.AddTelegram());

        /// <summary>
        /// Uses Telegram with the specified options.
        /// </summary>
        public BotCoreBuilder UseTelegram(TelegramConnectorOptions options)
            => builder.ConfigureConnectors(c => c.AddTelegram(options));

        /// <summary>
        /// Uses Telegram with the specified bot token and optional configurator.
        /// </summary>
        public BotCoreBuilder UseTelegram(
            string botToken,
            Action<TelegramConnectorOptions>? configure = null
        )
            => builder.ConfigureConnectors(c => c.AddTelegram(botToken, configure));
    }
}
