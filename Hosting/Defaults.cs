namespace DioRed.Vermilion.Hosting;
public static class Defaults
{
    public static string JobEventColor { get; set; } = "mediumspringgreen";

    public static string AzureAccountNameConfigurationKey { get; set; } = "Vermilion:AzureTable:AccountName";
    public static string AzureAccountKeyConfigurationKey { get; set; } = "Vermilion:AzureTable:AccountKey";

    public static TimeSpan ConsoleLoggerTimeZone { get; set; } = DateTimeOffset.Now.Offset;
}