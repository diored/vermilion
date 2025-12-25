namespace DioRed.Vermilion.Jobs;

public interface IScheduledJob
{
    ScheduledJobDefinition Definition { get; }
    Task Handle(IServiceProvider services, BotCore botCore, CancellationToken ct);
}