namespace DioRed.Vermilion;

public class BotOptions
{
    public string? Greeting { get; set; }
    public bool SaveChatTitles { get; set; } = true;
    public bool LogCommands { get; set; } = true;
    public bool ShowCoreVersion { get; set; } = true;
}