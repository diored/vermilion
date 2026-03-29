using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Connectors.Telegram;
using DioRed.Vermilion.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Hosting extensions for registering the Telegram connector.
/// </summary>
public static class TelegramHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:Telegram";

    extension(IConnectorsCollection connectors)
    {
        /// <summary>
        /// Adds Telegram connectors using configuration from the host configuration tree.
        /// </summary>
        public IConnectorsCollection AddTelegram()
        {
            return connectors.Add(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                Dictionary<string, IConnector> connectorsDictionary = [];

                string? accounts = configuration[$"{ConfigKeyPrefix}:Accounts"];

                if (accounts is null)
                {
                    TelegramConnectorOptions options = ReadOptions(
                        configuration,
                        ConfigKeyPrefix
                    );

                    options.ConnectorKey = Defaults.ConnectorKey;

                    connectorsDictionary.Add(
                        options.ConnectorKey,
                        new TelegramConnector(options, loggerFactory)
                     );
                }
                else
                {
                    foreach (string account in accounts.SplitBy(',').Distinct())
                    {
                        TelegramConnectorOptions options = ReadOptions(
                            configuration,
                            $"{ConfigKeyPrefix}:{account}"
                        );

                        options.ConnectorKey = account;

                        connectorsDictionary.Add(
                            options.ConnectorKey,
                            new TelegramConnector(options, loggerFactory)
                         );
                    }
                }

                return connectorsDictionary;
            });
        }

        /// <summary>
        /// Adds a Telegram connector using the specified options instance.
        /// </summary>
        public IConnectorsCollection AddTelegram(TelegramConnectorOptions options)
        {
            return connectors.Add(serviceProvider =>
            {
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                return new KeyValuePair<string, IConnector>(
                    Defaults.ConnectorKey,
                    new TelegramConnector(options, loggerFactory)
                );
            });
        }

        /// <summary>
        /// Adds a Telegram connector using the specified bot token and optional configurator.
        /// </summary>
        public IConnectorsCollection AddTelegram(
            string botToken,
            Action<TelegramConnectorOptions>? configure = null
        )
        {
            var options = new TelegramConnectorOptions
            {
                BotToken = botToken,
                ConnectorKey = Defaults.ConnectorKey,
            };

            configure?.Invoke(options);

            return connectors.AddTelegram(options);
        }
    }

    private static TelegramConnectorOptions ReadOptions(
        IConfiguration configuration,
        string prefix
    )
    {
        string botToken = configuration.GetRequiredValue($"{prefix}:BotToken");

        TelegramConnectorOptions options = new()
        {
            BotToken = botToken
        };

        if (configuration[$"{prefix}:SuperAdminId"] is { } superAdminId)
        {
            options.SuperAdmins = [long.Parse(superAdminId)];
        }
        else if (configuration[$"{prefix}:SuperAdmins"] is { } superAdminsIds)
        {
            options.SuperAdmins =
            [
                .. superAdminsIds.SplitBy(',').Select(long.Parse)
            ];
        }

        return options;
    }
}
