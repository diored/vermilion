namespace DioRed.Vermilion.ChatStorage;

public static class Extensions
{
    public static BotCoreBuilder UseInMemoryChatStorage(
        this BotCoreBuilder builder
    )
    {
        return builder.UseChatStorage(new InMemoryChatStorage());
    }
}