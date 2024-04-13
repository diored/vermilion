namespace DioRed.Vermilion.ChatStorage;

public interface IChatStorage
{
    Task AddChatAsync(ChatId chatId, string title);
    Task<ChatId[]> GetChatsAsync();
    Task RemoveChatAsync(ChatId chatId);
}