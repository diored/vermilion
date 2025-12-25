using DioRed.Common.Jobs;

namespace DioRed.Vermilion.Jobs;

public sealed class ScheduledJobDefinition
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[^12..];
    public required ISchedule Schedule { get; init; }
    public int? MaxOccurrences { get; init; }
    public MisfirePolicy MisfirePolicy { get; init; } = MisfirePolicy.FireOnce;
    public TimeSpan MisfireThreshold { get; init; } = TimeSpan.FromSeconds(1);
    public int MaxCatchUpExecutions { get; init; } = 10;
}