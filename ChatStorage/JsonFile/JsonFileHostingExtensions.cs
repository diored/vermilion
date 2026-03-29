using DioRed.Vermilion.ChatStorage;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Adds JSON file chat storage registration helpers.
/// </summary>
public static class JsonFileHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:JsonFile";

    extension(IChatStorageCollection chatStorageCollection)
    {
        /// <summary>
        /// Uses JSON file storage configured from the application configuration.
        /// </summary>
        public void UseJsonFile()
        {
            chatStorageCollection.Use(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                string filePath = configuration[$"{ConfigKeyPrefix}:FilePath"] ?? Defaults.FilePath;

                bool writeIndented = true;
                string? writeIndentedText = configuration[$"{ConfigKeyPrefix}:WriteIndented"];
                if (bool.TryParse(writeIndentedText, out bool parsed))
                {
                    writeIndented = parsed;
                }

                return new JsonFileChatStorage(filePath, writeIndented);
            });
        }

        /// <summary>
        /// Uses JSON file storage with the specified options.
        /// </summary>
        public void UseJsonFile(JsonFileChatStorageOptions options)
        {
            chatStorageCollection.Use(
                new JsonFileChatStorage(options.FilePath, options.WriteIndented)
            );
        }

        /// <summary>
        /// Uses JSON file storage with the specified file path and optional configuration.
        /// </summary>
        public void UseJsonFile(
            string filePath,
            Action<JsonFileChatStorageOptions>? configure = null
        )
        {
            var options = new JsonFileChatStorageOptions
            {
                FilePath = filePath,
                WriteIndented = true
            };

            configure?.Invoke(options);

            chatStorageCollection.UseJsonFile(options);
        }
    }

}
