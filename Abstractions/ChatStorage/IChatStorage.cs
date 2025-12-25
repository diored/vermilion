namespace DioRed.Vermilion.ChatStorage;

public interface IChatStorage
{
    Task AddChatAsync(ChatMetadata metadata);
    Task AddChatAsync(ChatMetadata metadata, string title);
    Task<ChatMetadata> GetChatAsync(ChatId chatId);
    Task<ChatMetadata[]> GetChatsAsync();
    Task RemoveChatAsync(ChatId chatId);
    Task UpdateChatAsync(ChatMetadata metadata);
}