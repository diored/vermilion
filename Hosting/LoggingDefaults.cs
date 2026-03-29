namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Provides defaults used by Vermilion console logging integration.
/// </summary>
public static class LoggingDefaults
{
    /// <summary>
    /// Gets or sets the console color used for scheduled job events.
    /// </summary>
    public static string JobEventColor { get; set; } = "mediumspringgreen";

    /// <summary>
    /// Gets or sets the time zone offset used by the console logger.
    /// </summary>
    public static TimeSpan ConsoleLoggerTimeZone { get; set; } = DateTimeOffset.Now.Offset;
}
