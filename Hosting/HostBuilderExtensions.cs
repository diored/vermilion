using DioRed.Common.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DioRed.Vermilion.Hosting;
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureVermilion(
        this IHostBuilder hostBuilder,
        string botName,
        Action<BotCoreBuilder> configureBuilder
    )
    {
        return hostBuilder
            .ConfigureLogging(logging =>
            {
                logging.UseDioRedLogging(
                    botName,
                    options =>
                    {
                        options.EventColors.Add(Events.JobStarted, LoggingDefaults.JobEventColor);
                        options.EventColors.Add(Events.JobFinished, LoggingDefaults.JobEventColor);
                        options.EventColors.Add(Events.JobScheduled, LoggingDefaults.JobEventColor);

                        options.DateTimeOffset = LoggingDefaults.ConsoleLoggerTimeZone;

                        options.ExceptionFormat = Spectre.Console.ExceptionFormats.ShortenEverything;
                    }
                );
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService(serviceProvider =>
                {
                    BotCoreBuilder botCoreBuilder = new(serviceProvider);
                    configureBuilder(botCoreBuilder);
                    return botCoreBuilder.Build();
                });
            });
    }
}