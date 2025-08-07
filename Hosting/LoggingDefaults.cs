namespace DioRed.Vermilion.Hosting;
public static class LoggingDefaults
{
    public static string JobEventColor { get; set; } = "mediumspringgreen";

    public static TimeSpan ConsoleLoggerTimeZone { get; set; } = DateTimeOffset.Now.Offset;
}