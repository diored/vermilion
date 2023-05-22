namespace DioRed.Vermilion;

[Flags]
public enum BotCommandOptions
{
    PlainText = 0,
    Regex = 2,
    CaseInsensitive = 64
}
