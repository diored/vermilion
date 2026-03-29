namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Persists chat metadata for connectors and the bot runtime.
/// </summary>
public interface IChatStorage
{
    /// <summary>
    /// Stores a new chat record.
    /// </summary>
    Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Loads a single chat by its identity.
    /// </summary>
    Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default);

    /// <summary>
    /// Streams all stored chats.
    /// </summary>
    IAsyncEnumerable<ChatMetadata> GetChatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Removes a chat by its identity.
    /// </summary>
    Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default);

    /// <summary>
    /// Replaces the stored metadata for an existing chat.
    /// </summary>
    Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default);
}
