using DioRed.Common.AzureStorage;
using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

public static class AzureTableHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:AzureTable";

    public static void UseAzureTable(
        this ChatStorageCollection chatStorageCollection
    )
    {
        chatStorageCollection.Use(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            AzureTableChatStorageOptions options = new()
            {
                Settings = AzureStorageSettings.Load(configuration.GetSection(ConfigKeyPrefix)),
                TableName = configuration[$"{ConfigKeyPrefix}:TableName"]
                    ?? Defaults.TableName
            };

            return new AzureTableChatStorage(
                options.Settings,
                options.TableName
            );
        });
    }

    public static void UseAzureTable(
        this ChatStorageCollection chatStorageCollection,
        AzureTableChatStorageOptions options
    )
    {
        chatStorageCollection.Use(
            new AzureTableChatStorage(
                options.Settings,
                options.TableName
            )
        );
    }
}