namespace DioRed.Vermilion.Jobs;

/// <summary>
/// Legacy daily job contract preserved for migration from older Vermilion versions.
/// </summary>
[Obsolete("Use IScheduledJob with ScheduledJobDefinition and LocalTimeDailySchedule instead.")]
public interface IDailyJob : IScheduledJob
{
    /// <summary>
    /// Gets the legacy daily job definition.
    /// </summary>
    new DailyJobDefinition Definition { get; }

    /// <summary>
    /// Executes the daily job.
    /// </summary>
    Task Handle(IServiceProvider services, BotCore botCore);

    ScheduledJobDefinition IScheduledJob.Definition => Definition.ToScheduledJobDefinition();

    Task IScheduledJob.HandleAsync(IServiceProvider services, BotCore botCore, CancellationToken ct)
        => Handle(services, botCore);
}
