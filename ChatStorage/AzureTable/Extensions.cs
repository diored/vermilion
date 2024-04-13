using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;

public static class Extensions
{
    public static BotCoreBuilder UseAzureTableChatStorage(
        this BotCoreBuilder builder,
        AzureStorageSettings settings,
        string tableName = "Chats"
    )
    {
        return builder.UseChatStorage(new AzureTableChatStorage(settings, tableName));
    }
}