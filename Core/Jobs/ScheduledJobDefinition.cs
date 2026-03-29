using DioRed.Common.Jobs;

namespace DioRed.Vermilion.Jobs;

/// <summary>
/// Describes how a scheduled job should run.
/// </summary>
public sealed class ScheduledJobDefinition
{
    /// <summary>
    /// Gets the job identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[^12..];

    /// <summary>
    /// Gets the underlying schedule.
    /// </summary>
    public required ISchedule Schedule { get; init; }

    /// <summary>
    /// Gets the optional maximum number of occurrences.
    /// </summary>
    public int? MaxOccurrences { get; init; }

    /// <summary>
    /// Gets the misfire policy.
    /// </summary>
    public MisfirePolicy MisfirePolicy { get; init; } = MisfirePolicy.FireOnce;

    /// <summary>
    /// Gets the threshold after which a missed occurrence counts as a misfire.
    /// </summary>
    public TimeSpan MisfireThreshold { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets the maximum number of catch-up executions after a misfire.
    /// </summary>
    public int MaxCatchUpExecutions { get; init; } = 10;
}
