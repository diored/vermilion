namespace DioRed.Vermilion;

[Flags]
public enum BotCommandOptions
{
    EqualsTo = 0,
    StartsWith = 1,
    Regex = 2,
    CaseInsensitive = 64
}
