namespace DioRed.Vermilion.Jobs;

public class DailyJobDefinition
{
    public string Id { get; init; } = Guid.NewGuid().ToString()[^12..];

    public required TimeOnly TimeOfDay { get; init; }
    public TimeSpan TimeZoneOffset { get; init; } = DateTimeOffset.Now.Offset;
    public int RepeatNumber { get; init; } = 0;
}