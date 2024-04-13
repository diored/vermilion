namespace DioRed.Vermilion;

public class BotOptions
{
    public string Greeting { get; set; } = "DioRED Vermilion Core {Version} is started.";
    public bool SaveChatTitles { get; set; } = true;
    public bool LogCommands { get; set; } = true;

    public BotOptions Clone()
    {
        return new BotOptions
        {
            Greeting = Greeting,
            SaveChatTitles = SaveChatTitles,
            LogCommands = LogCommands
        };
    }
}