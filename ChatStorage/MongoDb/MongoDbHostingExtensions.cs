using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Adds MongoDB chat storage registration helpers.
/// </summary>
public static class MongoDbHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:MongoDb";

    extension(IChatStorageCollection chatStorageCollection)
    {
        /// <summary>
        /// Uses MongoDB storage configured from the application configuration.
        /// </summary>
        public void UseMongoDb()
        {
            chatStorageCollection.Use(serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

                MongoDbChatStorageOptions options = new()
                {
                    ConnectionString = configuration.GetRequiredValue($"{ConfigKeyPrefix}:ConnectionString"),
                    DatabaseName = configuration[$"{ConfigKeyPrefix}:DatabaseName"] ?? Defaults.DatabaseName,
                    CollectionName = configuration[$"{ConfigKeyPrefix}:CollectionName"] ?? Defaults.CollectionName
                };

                return new MongoDbChatStorage(
                    options.ConnectionString,
                    options.DatabaseName,
                    options.CollectionName
                );
            });
        }

        /// <summary>
        /// Uses MongoDB storage with the specified options.
        /// </summary>
        public void UseMongoDb(MongoDbChatStorageOptions options)
        {
            chatStorageCollection.Use(
                new MongoDbChatStorage(
                    options.ConnectionString,
                    options.DatabaseName,
                    options.CollectionName
                )
            );
        }

        /// <summary>
        /// Uses MongoDB storage with the specified connection string and optional configuration.
        /// </summary>
        public void UseMongoDb(
            string connectionString,
            Action<MongoDbChatStorageOptions>? configure = null
        )
        {
            MongoDbChatStorageOptions options = new()
            {
                ConnectionString = connectionString,
                DatabaseName = Defaults.DatabaseName,
                CollectionName = Defaults.CollectionName
            };

            configure?.Invoke(options);

            chatStorageCollection.UseMongoDb(options);
        }
    }

}
