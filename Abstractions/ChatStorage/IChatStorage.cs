namespace DioRed.Vermilion.ChatStorage;

public interface IChatStorage
{
    Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    );
    Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default);
    IAsyncEnumerable<ChatMetadata> GetChatsAsync(CancellationToken ct = default);
    Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default);
    Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default);
}
