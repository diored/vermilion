namespace DioRed.Vermilion;

public class Job
{
    private readonly CancellationTokenSource _cancellation;

    private Job(string id)
    {
        Id = id;
        _cancellation = new CancellationTokenSource();
        Active = true;
    }

    public string Id { get; }
    public bool Active { get; private set; }

    public static Job SetupOneTime(ILogger logger, Func<Task> action, DateTime dateTime, string? id = default)
    {
        TimeSpan interval = dateTime - DateTime.UtcNow;
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "Cannot schedule to the past");
        }

        Job job = new(id ?? GenerateId());

        logger.LogInfo($"One-time job #{job.Id} is planned on {DateTime.UtcNow + interval} (in {interval}).");

        _ = Task.Run(async () =>
        {
            await Task.Delay(interval, job._cancellation.Token);

            logger.LogInfo($"Job #{job.Id} started.");
            await action();
            logger.LogInfo($"Job #{job.Id} finished.");

            job.Active = false;
        }, job._cancellation.Token);

        return job;
    }

    public static Job SetupDaily(ILogger logger, Func<Task> action, TimeSpan time, string? id = default)
    {
        var firstOccurrence = DateTime.UtcNow.Date + time;
        if (firstOccurrence <= DateTime.UtcNow)
        {
            firstOccurrence += TimeSpan.FromDays(1);
        }

        return SetupRepeat(logger, action, TimeSpan.FromDays(1), firstOccurrence, id);
    }

    public static Job SetupRepeat(ILogger logger, Func<Task> action, TimeSpan repeatInterval, DateTime? firstOccurrence = default, string? id = default)
    {
        TimeSpan interval;

        if (firstOccurrence.HasValue)
        {
            if (firstOccurrence < DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(firstOccurrence), $"Cannot schedule to the past: {firstOccurrence} is before {DateTime.UtcNow}");
            }

            interval = firstOccurrence.Value - DateTime.UtcNow;
        }
        else
        {
            interval = TimeSpan.Zero;
        }

        Job job = new(id ?? GenerateId());

        _ = Task.Run(async () =>
        {
            while (true)
            {
                logger.LogInfo($"Repeatable job #{job.Id} is planned on {DateTime.UtcNow + interval} (in {interval})");

                await Task.Delay(interval, job._cancellation.Token);

                logger.LogInfo($"Job #{job.Id} started.");
                await action();
                logger.LogInfo($"Job #{job.Id} finished.");

                interval = repeatInterval;
            }

        }, job._cancellation.Token);

        return job;
    }

    public void Cancel()
    {
        _cancellation.Cancel();
        Active = false;
    }

    private static string GenerateId()
    {
        return Guid.NewGuid().ToString()[^12..];
    }
}