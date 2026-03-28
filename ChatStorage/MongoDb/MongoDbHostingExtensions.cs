using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

public static class MongoDbHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:MongoDb";

    extension(IChatStorageCollection chatStorageCollection)
    {
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
