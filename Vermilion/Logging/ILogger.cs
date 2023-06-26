namespace DioRed.Vermilion.Logging;

public interface ILogger
{
    void LogError(string message);
    void LogInfo(string message);
}