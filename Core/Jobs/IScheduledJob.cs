namespace DioRed.Vermilion.Jobs;

/// <summary>
/// Represents a scheduled background job that can interact with the bot runtime.
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// Gets the job definition.
    /// </summary>
    ScheduledJobDefinition Definition { get; }

    /// <summary>
    /// Executes the scheduled job.
    /// </summary>
    Task Handle(IServiceProvider services, BotCore botCore, CancellationToken ct);
}
