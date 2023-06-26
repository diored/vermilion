namespace DioRed.Vermilion.Handlers;

[Flags]
public enum BotCommandOptions
{
    PlainText = 0,
    Regex = 2,
    CaseInsensitive = 64
}
