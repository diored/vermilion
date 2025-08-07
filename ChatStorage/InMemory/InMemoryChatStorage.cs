using DioRed.Vermilion.ChatStorage.L10n;

namespace DioRed.Vermilion.ChatStorage;

public class InMemoryChatStorage : IChatStorage
{
    private readonly HashSet<ChatMetadata> _chats = [];

    public Task AddChatAsync(ChatMetadata metadata)
    {
        return AddChatAsync(metadata, string.Empty);
    }

    public Task AddChatAsync(ChatMetadata metadata, string title)
    {
        if (!_chats.Add(metadata))
        {
            throw new InvalidOperationException(
                ExceptionMessages.ChatAlreadyStored_0
            );
        }

        return Task.CompletedTask;
    }

    public Task<ChatMetadata> GetChatAsync(ChatId chatId)
    {
        return Task.FromResult(_chats.First(chat => chat.ChatId == chatId));
    }

    public Task<ChatMetadata[]> GetChatsAsync()
    {
        return Task.FromResult(_chats.ToArray());
    }

    public Task RemoveChatAsync(ChatId chatId)
    {
        _ = _chats.RemoveWhere(chatInfo => chatInfo.ChatId == chatId);

        return Task.CompletedTask;
    }

    public Task UpdateChatAsync(ChatMetadata metadata)
    {
        ChatMetadata? existing = _chats.FirstOrDefault(chat => chat.ChatId == metadata.ChatId)
            ?? throw new ArgumentException(
                message: $"Chat {metadata.ChatId} not found",
                paramName: nameof(metadata)
            );

        _chats.Remove(existing);
        _chats.Add(metadata);

        return Task.CompletedTask;
    }
}