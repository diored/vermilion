namespace DioRed.Vermilion.ChatStorage;

public interface IChatStorage
{
    Task AddChatAsync(ChatInfo chatInfo, string title);
    Task<ChatInfo> GetChatAsync(ChatId chatId);
    Task<ChatInfo[]> GetChatsAsync();
    Task RemoveChatAsync(ChatId chatId);
    Task UpdateChatAsync(ChatInfo chatInfo);
}