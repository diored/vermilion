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

    public Task<ChatInfo[]> GetChatsAsync()
    {
        return Task.FromResult(_chats.ToArray());
    }

    public Task RemoveChatAsync(ChatId chatId)
    {
        _ = _chats.RemoveWhere(chatInfo => chatInfo.ChatId == chatId);

        return Task.CompletedTask;
    }
}