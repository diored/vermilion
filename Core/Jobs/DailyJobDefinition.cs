using DioRed.Common.Jobs;

namespace DioRed.Vermilion.Jobs;

/// <summary>
/// Legacy daily job definition preserved for migration from older Vermilion versions.
/// </summary>
[Obsolete("Use ScheduledJobDefinition with LocalTimeDailySchedule instead.")]
public sealed class DailyJobDefinition
{
    /// <summary>
    /// Gets the job identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[^12..];

    /// <summary>
    /// Gets the local time of day when the job should run.
    /// </summary>
    public required TimeOnly TimeOfDay { get; init; }

    internal ScheduledJobDefinition ToScheduledJobDefinition()
    {
        return new ScheduledJobDefinition
        {
            Id = Id,
            Schedule = new LocalTimeDailySchedule(TimeOfDay)
        };
    }
}
