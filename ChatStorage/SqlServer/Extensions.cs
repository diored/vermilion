namespace DioRed.Vermilion.ChatStorage;

public static class Extensions
{
    public static BotCoreBuilder UseSqlServerChatStorage(
        this BotCoreBuilder builder,
        string connectionString,
        string table = "Chats",
        string schema = "dbo"
    )
    {
        return builder.UseChatStorage(new SqlServerChatStorage(connectionString, table, schema));
    }
}