using DioRed.Common.AzureStorage;
using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

public static class AzureTableHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:AzureTable";

    extension(IChatStorageCollection chatStorageCollection)
    {
        public void UseAzureTable()
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

        public void UseAzureTable(AzureTableChatStorageOptions options)
        {
            chatStorageCollection.Use(
                new AzureTableChatStorage(
                    options.Settings,
                    options.TableName
                )
            );
        }

        public void UseAzureTable(
            AzureStorageSettings settings,
            Action<AzureTableChatStorageOptions>? configure = null
        )
        {
            var options = new AzureTableChatStorageOptions
            {
                Settings = settings,
                TableName = Defaults.TableName
            };

            configure?.Invoke(options);

            chatStorageCollection.UseAzureTable(options);
        }
    }
}