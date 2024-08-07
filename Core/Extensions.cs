using DioRed.Common.Jobs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

public static class Extensions
{
    public static IServiceCollection AddVermilion(
        this IServiceCollection services,
        Action<BotCoreBuilder> setup
    )
    {
        return services
            .AddHostedService(serviceProvider =>
            {
                var botCore = BotCore.CreateBuilder(serviceProvider);
                setup?.Invoke(botCore);
                return botCore.Build();
            });
    }

    public static IServiceProvider SetupDailyJob(
        this IServiceProvider services,
        Func<IServiceProvider, BotCore, Task> action,
        TimeOnly timeOfDay,
        TimeSpan timeZoneOffset,
        int repeatNumber = 0,
        string? id = null
    )
    {
        BotCore botCore = services.GetServices<IHostedService>()
            .OfType<BotCore>()
            .Single();

        ILogger<BotCore> logger = services.GetRequiredService<ILogger<BotCore>>();

        var job = Job.SetupDaily(
            () => action(services, botCore),
            timeOfDay,
            timeZoneOffset,
            repeatNumber,
            id
        );

        job.Started += (_, _) => logger.LogInformation(
            Events.JobStarted,
            """Job "{JobId}" started""",
            job.Id
        );

        job.Finished += (_, _) => logger.LogInformation(
            Events.JobFinished,
            """Job "{JobId}" finished""",
            job.Id
        );

        job.Scheduled += (_, eventArgs) => logger.LogInformation(
            Events.JobScheduled,
            """Next occurrence (#{OcurrenceNumber}) of the job "{JobId}" is scheduled at {NextOccurrence} (in {TimeLeft})""",
            eventArgs.OccurrenceNumber,
            job.Id,
            eventArgs.NextOccurrence.ToString("u"),
            (eventArgs.NextOccurrence - DateTimeOffset.Now).ToString("c")
        );

        job.Start();

        return services;
    }
}