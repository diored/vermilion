namespace DioRed.Vermilion.ChatStorage;

public class InMemoryChatStorage : IChatStorage
{
    private readonly Dictionary<ChatId, ChatMetadata> _chats = [];

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

    public Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _chats.TryGetValue(chatId, out ChatMetadata? chat)
            ? Task.FromResult(chat)
            : throw new ChatNotFoundException(chatId);
    }

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

    public Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _chats.Remove(chatId);

        return Task.CompletedTask;
    }

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
