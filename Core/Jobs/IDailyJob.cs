namespace DioRed.Vermilion.Jobs;

public interface IDailyJob
{
    DailyJobDefinition Definition { get; }
    Task Handle(IServiceProvider services, BotCore botCore);
}