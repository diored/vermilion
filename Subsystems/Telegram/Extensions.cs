using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion.Subsystems.Telegram;

public static class Extensions
{
    private static class ConfigKeys
    {
        private const string _prefix = "Vermilion:Telegram:";

        public const string BotToken = $"{_prefix}BotToken";
        public const string SuperAdminId = $"{_prefix}SuperAdminId";
        public const string SuperAdmins = $"{_prefix}SuperAdmins";
    }

    public static BotCoreBuilder AddTelegram(
        this BotCoreBuilder builder
    )
    {
        var configuration = builder.Services.GetRequiredService<IConfiguration>();

        string botToken = configuration[ConfigKeys.BotToken]
            ?? throw new InvalidOperationException(
                $"Cannot read {ConfigKeys.BotToken} value from the configuration"
            );

        long[] superAdmins;
        if (configuration[ConfigKeys.SuperAdminId] is { } superAdminId)
        {
            superAdmins = [long.Parse(superAdminId)];
        }
        else if (configuration[ConfigKeys.SuperAdmins] is { } superAdminsIds)
        {
            superAdmins = [.. superAdminsIds.Split(',').Select(long.Parse)];
        }
        else
        {
            superAdmins = [];
        }

        return builder.AddTelegram(
            botToken,
            superAdmins
        );
    }

    public static BotCoreBuilder AddTelegram(
        this BotCoreBuilder builder,
        string botToken,
        long[]? superAdmins = null,
        string? system = null
    )
    {
        TelegramSubsystem subsystem = new(
            new TelegramSubsystemOptions
            {
                BotToken = botToken,
                SuperAdmins = superAdmins ?? []
            },
            builder.Services.GetRequiredService<ILoggerFactory>()
        );

        return builder.AddSubsystem(
            system ?? TelegramDefaults.System,
            subsystem
        );
    }
}