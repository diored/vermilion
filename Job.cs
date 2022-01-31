namespace DioRed.Vermilion;

public class Job
{
    private readonly CancellationTokenSource _cancellation;

    private Job()
    {
        _cancellation = new CancellationTokenSource();
        Active = true;
    }

    public string Id { get; } = Guid.NewGuid().ToString()[^12..];
    public bool Active { get; private set; }

    public static Job SetupOneTime(Bot bot, Func<Task> action, DateTime dateTime)
    {
        TimeSpan interval = dateTime - DateTime.UtcNow;
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException("Cannot schedule to the past");
        }

        Job job = new();

        bot.LogInfo($"One-time job #{job.Id} is planned on {DateTime.UtcNow + interval} (in {interval}).");

        _ = Task.Run(async () =>
        {
            await Task.Delay(interval, job._cancellation.Token);

            bot.LogInfo($"Job #{job.Id} started.");
            await action();
            bot.LogInfo($"Job #{job.Id} finished.");

            job.Active = false;
        }, job._cancellation.Token);

        return job;
    }

    public static Job SetupDaily(Bot bot, Func<Task> action, TimeSpan time)
    {
        var firstOccurrence = DateTime.UtcNow.Date + time;
        if (firstOccurrence <= DateTime.UtcNow)
        {
            firstOccurrence += TimeSpan.FromDays(1);
        }

        return SetupRepeat(bot, action, TimeSpan.FromDays(1), firstOccurrence);
    }

    public static Job SetupRepeat(Bot bot, Func<Task> action, TimeSpan repeatInterval, DateTime? firstOccurrence = default)
    {
        TimeSpan interval;

        if (firstOccurrence.HasValue)
        {
            if (firstOccurrence < DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException($"Cannot schedule to the past: {firstOccurrence} is before {DateTime.UtcNow}");
            }

            interval = firstOccurrence.Value - DateTime.UtcNow;
        }
        else
        {
            interval = TimeSpan.Zero;
        }

        Job job = new();

        _ = Task.Run(async () =>
        {
            while (true)
            {
                bot.LogInfo($"Repeatable job #{job.Id} is planned on {DateTime.UtcNow + interval} (in {interval})");

                await Task.Delay(interval, job._cancellation.Token);                
                
                bot.LogInfo($"Job #{job.Id} started.");
                await action();
                bot.LogInfo($"Job #{job.Id} finished.");
                
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
}
