using DioRed.Common.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DioRed.Vermilion.Hosting;
public static class HostBuilderExtensions
{
    /// <summary>
    /// Registers Vermilion in a host in a DI-friendly way: the configuration delegate is executed
    /// during <c>ConfigureServices</c>, so plugins can add their own services, while Vermilion
    /// configuration itself is deferred until the container is built.
    /// </summary>
    public static IHostBuilder AddVermilion(
        this IHostBuilder hostBuilder,
        string botName,
        Action<VermilionBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

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
                var builder = new VermilionBuilder(services);
                configure(builder);

                services.AddHostedService(serviceProvider =>
                {
                    BotCoreBuilder botCoreBuilder = new(serviceProvider);
                    foreach (var c in builder.BotCoreConfigurators)
                    {
                        c(botCoreBuilder);
                    }
                    return botCoreBuilder.Build();
                });
            });
    }

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