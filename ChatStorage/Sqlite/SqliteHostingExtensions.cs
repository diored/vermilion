using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

public static class SqliteHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:Sqlite";

    extension(IChatStorageCollection chatStorageCollection)
    {
        public void UseSqlite()
        {
            chatStorageCollection.Use(serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

                SqliteChatStorageOptions options = new()
                {
                    ConnectionString = configuration.GetRequiredValue($"{ConfigKeyPrefix}:ConnectionString"),
                    TableName = configuration[$"{ConfigKeyPrefix}:TableName"] ?? Defaults.TableName
                };

                return new SqliteChatStorage(
                    options.ConnectionString,
                    options.TableName
                );
            });
        }

        public void UseSqlite(SqliteChatStorageOptions options)
        {
            chatStorageCollection.Use(
                new SqliteChatStorage(
                    options.ConnectionString,
                    options.TableName
                )
            );
        }

        public void UseSqlite(
            string connectionString,
            Action<SqliteChatStorageOptions>? configure = null
        )
        {
            SqliteChatStorageOptions options = new()
            {
                ConnectionString = connectionString,
                TableName = Defaults.TableName
            };

            configure?.Invoke(options);

            chatStorageCollection.UseSqlite(options);
        }
    }
}
