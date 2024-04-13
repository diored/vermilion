namespace DioRed.Vermilion.ChatStorage;

public class InMemoryChatStorage : IChatStorage
{
    private readonly HashSet<ChatId> _chats = [];

    public Task AddChatAsync(ChatId chatId, string title)
    {
        if (!_chats.Add(chatId))
        {
            throw new InvalidOperationException("Chat already presents in the storage");
        }

        return Task.CompletedTask;
    }

    public Task<ChatId[]> GetChatsAsync()
    {
        return Task.FromResult(_chats.ToArray());
    }

    public Task RemoveChatAsync(ChatId chatId)
    {
        _ = _chats.Remove(chatId);

        return Task.CompletedTask;
    }
}