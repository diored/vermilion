using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Connectors.Telegram;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion.Hosting;

public static class TelegramHostingExtensions
{
    private const string ConfigKeyPrefix = "Vermilion:Telegram";

    public static ConnectorsCollection AddTelegram(
        this ConnectorsCollection connectors
    )
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

    public static ConnectorsCollection AddTelegram(
        this ConnectorsCollection connectors,
        TelegramConnectorOptions options
    )
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