using System.Reflection;

using DioRed.Common.AzureStorage;
using DioRed.Common.Logging;
using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Subsystems.Telegram;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DioRed.Vermilion.Hosting;

public static class Extensions
{
    public static IHost BuildDefaultVermilionHost(
        this IHostBuilder hostBuilder,
        string botName,
        Action<HostBuilderContext, IServiceCollection>? configureServices = null,
        IEnumerable<Assembly>? assembliesWithHandlers = null,
        Action<BotCoreBuilder>? configureBuilder = null
    )
    {
        return hostBuilder
            .ConfigureLogging(logging => logging.UseDioRedLogging(
                botName,
                options =>
                {
                    options.EventColors.Add(Events.JobStarted, Defaults.JobEventColor);
                    options.EventColors.Add(Events.JobFinished, Defaults.JobEventColor);
                    options.EventColors.Add(Events.JobScheduled, Defaults.JobEventColor);

                    options.DateTimeOffset = Defaults.ConsoleLoggerTimeZone;

                    options.ExceptionFormat = Spectre.Console.ExceptionFormats.ShortenEverything;
                }
            )
        )
        .ConfigureServices((context, services) =>
        {
            configureServices?.Invoke(context, services);

            services.AddVermilion(builder =>
            {
                builder.UseAzureTableChatStorage(AzureStorageSettings.MicrosoftAzure(
                    accountName: ReadRequired(context.Configuration, Defaults.AzureAccountNameConfigurationKey),
                    accountKey: ReadRequired(context.Configuration, Defaults.AzureAccountKeyConfigurationKey)
                ));

                assembliesWithHandlers ??= [Assembly.GetEntryAssembly()!];

                foreach (var assembly in assembliesWithHandlers)
                {
                    builder.AddCommandHandlersFromAssembly(assembly);
                }

                builder.AddTelegram();

                configureBuilder?.Invoke(builder);
            });
        })
        .Build();
    }

    private static string ReadRequired(IConfiguration configuration, string keyName)
    {
        return configuration[keyName]
            ?? throw new InvalidOperationException($"""Cannot read "{keyName}" value""");
    }
}