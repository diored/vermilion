using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Adds SQLite chat storage registration helpers.
/// </summary>
public static class SqliteHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:Sqlite";

    extension(IChatStorageCollection chatStorageCollection)
    {
        /// <summary>
        /// Uses SQLite storage configured from the application configuration.
        /// </summary>
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

        /// <summary>
        /// Uses SQLite storage with the specified options.
        /// </summary>
        public void UseSqlite(SqliteChatStorageOptions options)
        {
            chatStorageCollection.Use(
                new SqliteChatStorage(
                    options.ConnectionString,
                    options.TableName
                )
            );
        }

        /// <summary>
        /// Uses SQLite storage with the specified connection string and optional configuration.
        /// </summary>
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
