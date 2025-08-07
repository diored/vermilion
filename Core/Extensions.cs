using DioRed.Common.Jobs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

public static class Extensions
{
    /// <summary>
    /// Sets up a daily job that executes the specified action at a given time of day and time zone offset.
    /// </summary>
    /// <param name="services">The service provider to resolve dependencies.</param>
    /// <param name="action">The action to execute for the job.</param>
    /// <param name="timeOfDay">The time of day to run the job.</param>
    /// <param name="timeZoneOffset">The time zone offset for scheduling.</param>
    /// <param name="repeatNumber">The number of times to repeat the job (default is 0).</param>
    /// <param name="id">An optional identifier for the job.</param>
    /// <returns>The IServiceProvider for chaining.</returns>
    public static IServiceProvider SetupDailyJob(
        this IServiceProvider services,
        Func<IServiceProvider, BotCore, Task> action,
        TimeOnly timeOfDay,
        TimeSpan timeZoneOffset,
        int repeatNumber = 0,
        string? id = null
    )
    {
        ILogger<BotCore> logger = services.GetRequiredService<ILogger<BotCore>>();

        var job = Job.SetupDaily(
            () =>
            {
                BotCore botCore = services.GetServices<IHostedService>()
                    .OfType<BotCore>()
                    .Single();

                return action(services, botCore);
            },
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

    /// <summary>
    /// Normalizes a Version object to a string, removing trailing ".0" segments.
    /// </summary>
    /// <param name="version">The version to normalize.</param>
    /// <returns>A normalized version string.</returns>
    public static string Normalize(this Version? version)
    {
        return version?.ToString() switch
        {
            null => "0.0",
            var v when v.EndsWith(".0.0") => v[..^4],
            var v when v.EndsWith(".0") => v[..^2],
            var v => v
        };
    }

    /// <summary>
    /// Splits a string by the specified separator, removing empty entries and trimming whitespace.
    /// </summary>
    /// <param name="text">The string to split.</param>
    /// <param name="separator">The character to split by.</param>
    /// <returns>An array of split and trimmed strings.</returns>
    public static string[] SplitBy(this string text, char separator)
    {
        return text.Split(
            separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
    }
}