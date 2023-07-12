namespace DioRed.Vermilion;

public class BotBlockedException : Exception
{
    public BotBlockedException()
        : base("Bot blocked")
    {
    }

    public BotBlockedException(Exception? innerException)
        : base("Bot blocked", innerException)
    {
    }
}