using DioRed.Common.AzureStorage;
using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;

namespace DioRed.Vermilion.Tools.ChatStorageMigrator;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            IConfigurationRoot configuration = BuildConfiguration(args);
            IConfigurationSection root = configuration.GetRequiredSection("VermilionMigration");

            IChatStorage source = CreateStorage(root.GetRequiredSection("Source"));
            IChatStorage target = CreateStorage(root.GetRequiredSection("Target"));

            Console.WriteLine("Starting chat storage migration.");
            Console.WriteLine($"Source: {root.GetRequiredSection("Source")["Provider"]}");
            Console.WriteLine($"Target: {root.GetRequiredSection("Target")["Provider"]}");
            Console.WriteLine("Target storage must be empty.");

            ChatStorageMigrationResult result = await DioRed.Vermilion.ChatStorage.ChatStorageMigrator.MigrateAsync(source, target);

            Console.WriteLine($"Migration completed successfully. Migrated chats: {result.MigratedChats}.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Migration failed.");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static IConfigurationRoot BuildConfiguration(string[] args)
    {
        string? configPath = GetOptionValue(args, "--config");
        string basePath = Directory.GetCurrentDirectory();

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);

        if (!string.IsNullOrWhiteSpace(configPath))
        {
            string fullPath = Path.GetFullPath(configPath, basePath);
            builder.AddJsonFile(fullPath, optional: false, reloadOnChange: false);
        }

        return builder
            .AddEnvironmentVariables(prefix: "VERMILION_MIGRATOR_")
            .AddCommandLine(args)
            .Build();
    }

    private static string? GetOptionValue(IReadOnlyList<string> args, string optionName)
    {
        for (int i = 0; i < args.Count - 1; i++)
        {
            if (string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static IChatStorage CreateStorage(IConfigurationSection section)
    {
        string provider = section["Provider"]
            ?? throw new InvalidOperationException("Storage provider is required.");

        return provider.Trim().ToLowerInvariant() switch
        {
            "azuretable" or "azure-table" => CreateAzureTableStorage(section.GetRequiredSection("AzureTable")),
            "mongodb" or "mongo" or "mongo-db" => CreateMongoDbStorage(section.GetRequiredSection("MongoDb")),
            "sqlite" => CreateSqliteStorage(section.GetRequiredSection("Sqlite")),
            "sqlserver" or "sql-server" => CreateSqlServerStorage(section.GetRequiredSection("SqlServer")),
            "jsonfile" or "json-file" => CreateJsonFileStorage(section.GetRequiredSection("JsonFile")),
            "inmemory" or "in-memory" => new InMemoryChatStorage(),
            _ => throw new InvalidOperationException($"Unsupported storage provider '{provider}'.")
        };
    }

    private static IChatStorage CreateAzureTableStorage(IConfigurationSection section)
    {
        AzureStorageSettings settings = AzureStorageSettings.Load(section);
        string tableName = section["TableName"] ?? "Chats";

        return new AzureTableChatStorage(settings, tableName);
    }

    private static IChatStorage CreateMongoDbStorage(IConfigurationSection section)
    {
        string connectionString = section["ConnectionString"]
            ?? throw new InvalidOperationException("MongoDb:ConnectionString is required.");

        return new MongoDbChatStorage(
            connectionString,
            section["DatabaseName"] ?? "Vermilion",
            section["CollectionName"] ?? "Chats"
        );
    }

    private static IChatStorage CreateSqliteStorage(IConfigurationSection section)
    {
        string connectionString = section["ConnectionString"]
            ?? throw new InvalidOperationException("Sqlite:ConnectionString is required.");

        return new SqliteChatStorage(
            connectionString,
            section["TableName"] ?? "Chats"
        );
    }

    private static IChatStorage CreateSqlServerStorage(IConfigurationSection section)
    {
        string connectionString = section["ConnectionString"]
            ?? throw new InvalidOperationException("SqlServer:ConnectionString is required.");

        return new SqlServerChatStorage(
            connectionString,
            section["TableName"] ?? "Chats",
            section["Schema"] ?? "dbo"
        );
    }

    private static IChatStorage CreateJsonFileStorage(IConfigurationSection section)
    {
        string filePath = section["FilePath"]
            ?? throw new InvalidOperationException("JsonFile:FilePath is required.");

        bool writeIndented = section.GetValue<bool?>("WriteIndented") ?? true;
        return new JsonFileChatStorage(filePath, writeIndented);
    }
}
