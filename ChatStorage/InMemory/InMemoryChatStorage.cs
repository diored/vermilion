namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Stores chat metadata in memory for the lifetime of the current process.
/// </summary>
public class InMemoryChatStorage : IChatStorage, IChatStorageExport
{
    private readonly Dictionary<ChatId, ChatMetadata> _chats = [];

    /// <inheritdoc />
    public Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();
        if (!_chats.TryAdd(metadata.ChatId, metadata))
        {
            throw new ChatAlreadyExistsException(metadata.ChatId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _chats.TryGetValue(chatId, out ChatMetadata? chat)
            ? Task.FromResult(chat)
            : throw new ChatNotFoundException(chatId);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default
    )
    {
        foreach (ChatMetadata chat in _chats.Values)
        {
            ct.ThrowIfCancellationRequested();
            yield return chat;
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StoredChatRecord> ExportChatsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default
    )
    {
        foreach (ChatMetadata chat in _chats.Values)
        {
            ct.ThrowIfCancellationRequested();
            yield return new StoredChatRecord
            {
                Metadata = chat
            };
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _chats.Remove(chatId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!_chats.ContainsKey(metadata.ChatId))
        {
            throw new ChatNotFoundException(metadata.ChatId);
        }

        _chats[metadata.ChatId] = metadata;

        return Task.CompletedTask;
    }
}
