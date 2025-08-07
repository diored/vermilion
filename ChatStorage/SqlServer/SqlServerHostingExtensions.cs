using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

public static class SqlServerHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:SqlServer";

    public static void UseSqlServer(
        this ChatStorageCollection chatStorageCollection
    )
    {
        chatStorageCollection.Use(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            SqlServerChatStorageOptions options = new()
            {
                ConnectionString = configuration.GetRequiredValue($"{ConfigKeyPrefix}:ConnectionString"),
                TableName = configuration[$"{ConfigKeyPrefix}:TableName"] ?? Defaults.TableName,
                Schema = configuration[$"{ConfigKeyPrefix}:Schema"] ?? Defaults.Schema
            };

            return new SqlServerChatStorage(
                options.ConnectionString,
                options.TableName,
                options.Schema
            );
        });
    }

    public static void UseSqlServer(
        this ChatStorageCollection chatStorageCollection,
        SqlServerChatStorageOptions options
    )
    {
        chatStorageCollection.Use(
            new SqlServerChatStorage(
                options.ConnectionString,
                options.TableName,
                options.Schema
            )
        );
    }
}