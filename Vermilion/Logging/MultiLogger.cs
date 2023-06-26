namespace DioRed.Vermilion.Logging;

public class MultiLogger : ILogger
{
    public ISet<ILogger> Loggers { get; } = new HashSet<ILogger>();

    public void LogInfo(string message)
    {
        foreach (var logger in Loggers)
        {
            logger.LogInfo(message);
        }
    }

    public void LogError(string message)
    {
        foreach (var logger in Loggers)
        {
            logger.LogError(message);
        }
    }
}