using DioRed.Common.Jobs;
using DioRed.Vermilion.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion.Extensions;

public static class ServiceProviderExtensions
{
    public static IServiceProvider SetupDailyJob(
        this IServiceProvider services,
        Func<IServiceProvider, BotCore, CancellationToken, Task> action,
        TimeOnly timeOfDay,
        TimeSpan timeZoneOffset,
        int? maxOccurrences = null,
        string? id = null
    )
    {
        var logger = services.GetRequiredService<ILogger<BotCore>>();
        var appLifetime = services.GetRequiredService<IHostApplicationLifetime>();

        DailySchedule schedule = new(
            timeOfDay: timeOfDay.ToTimeSpan(),
            timeZoneOffset: timeZoneOffset
        );

        JobOptions options = new()
        {
            MaxOccurrences = maxOccurrences
        };

        Job job = new(
            action: async ct =>
            {
                var botCore = services.GetServices<IHostedService>()
                    .OfType<BotCore>()
                    .Single();

                await action(services, botCore, ct).ConfigureAwait(false);
            },
            schedule: schedule,
            options: options
        );

        string jobId = id ?? job.Id;

        job.OccurrenceStarted += (_, e) => logger.LogInformation(
            Events.JobStarted,
            """Job "{JobId}" started (#{OccurrenceNumber})""",
            jobId,
            e.OccurrenceNumber
        );

        job.OccurrenceFinished += (_, e) => logger.LogInformation(
            Events.JobFinished,
            """Job "{JobId}" finished (#{OccurrenceNumber}) in {Duration}""",
            jobId,
            e.OccurrenceNumber,
            e.Duration.ToString("c")
        );

        job.Scheduled += (_, e) => logger.LogInformation(
            Events.JobScheduled,
            """Next occurrence (#{OccurrenceNumber}) of the job "{JobId}" is scheduled at {NextOccurrence} (in {TimeLeft})""",
            e.OccurrenceNumber,
            jobId,
            e.NextOccurrence.ToString("u"),
            (e.NextOccurrence - DateTimeOffset.Now).ToString("c")
        );

        job.Faulted += (_, e) => logger.LogError(
            Events.JobFailed,
            e.Exception,
            """Job "{JobId}" faulted""",
            jobId
        );

        job.Cancelled += (_, _) => logger.LogInformation(
            Events.JobCancelled,
            """Job "{JobId}" cancelled""",
            jobId
        );

        job.Completed += (_, _) => logger.LogInformation(
            Events.JobCompleted,
            """Job "{JobId}" completed""",
            jobId
        );

        _ = RunJob(job, logger, appLifetime.ApplicationStopping);

        return services;
    }

    private static async Task RunJob(Job job, ILogger logger, CancellationToken ct = default)
    {
        try
        {
            await job.StartAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // ok
        }
        catch (Exception ex)
        {
            logger.LogError(ex, """Job "{JobId}" crashed in RunJob wrapper""", job.Id);
        }
    }
}