using DioRed.Vermilion.ChatStorage.L10n;

namespace DioRed.Vermilion.ChatStorage;

public class InMemoryChatStorage : IChatStorage
{
    private readonly HashSet<ChatInfo> _chats = [];

    public Task AddChatAsync(ChatInfo chatInfo, string title)
    {
        if (!_chats.Add(chatInfo))
        {
            throw new InvalidOperationException(
                ExceptionMessages.ChatAlreadyStored_0
            );
        }

        return Task.CompletedTask;
    }

    public Task<ChatInfo> GetChatAsync(ChatId chatId)
    {
        return Task.FromResult(_chats.First(chat => chat.ChatId == chatId));
    }

    public Task<ChatInfo[]> GetChatsAsync()
    {
        return Task.FromResult(_chats.ToArray());
    }

    public Task RemoveChatAsync(ChatId chatId)
    {
        _ = _chats.RemoveWhere(chatInfo => chatInfo.ChatId == chatId);

        return Task.CompletedTask;
    }

    public Task UpdateChatAsync(ChatInfo chatInfo)
    {
        ChatInfo? existing = _chats.FirstOrDefault(chat => chat.ChatId == chatInfo.ChatId);

        if (existing is null)
        {
            throw new ArgumentException(
                message: $"Chat {chatInfo.ChatId} not found",
                paramName: nameof(chatInfo)
            );
        }

        _chats.Remove(existing);
        _chats.Add(chatInfo);

        return Task.CompletedTask;
    }
}