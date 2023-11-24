using System.Collections.Concurrent;

namespace DioRed.Vermilion;

public class ChatManager(IChatStorage chatStorage) : IChatManager
{
    private readonly ConcurrentDictionary<ChatId, ChatClient> _chatClients = new();

    public ChatClient? GetClient(ChatId chatId)
    {
        _chatClients.TryGetValue(chatId, out ChatClient? chatClient);
        return chatClient;
    }

    public ICollection<ChatClient> GetAllClients()
    {
        return _chatClients.Values.ToList();
    }

    public ICollection<ChatId> GetStoredChats(BotSystem system)
    {
        return chatStorage.GetChats()
            .Where(chatId => chatId.System == system)
            .ToList();
    }

    public ChatClient AddChatClient(ChatId chatId, ChatClient chatClient)
    {
        (chatClient, _) = AddChatClientInternal(chatId, chatClient);
        return chatClient;
    }

    public ChatClient AddAndStoreChatClient(ChatId chatId, ChatClient chatClient, string title)
    {
        (chatClient, bool added) = AddChatClientInternal(chatId, chatClient);

        if (added)
        {
            chatStorage.AddChat(chatId, title);
        }

        return chatClient;
    }

    public void Remove(ChatId chatId)
    {
        chatStorage.RemoveChat(chatId);
        _chatClients.Remove(chatId, out _);
    }

    private (ChatClient ChatClient, bool Added) AddChatClientInternal(ChatId chatId, ChatClient chatClient)
    {
        if (_chatClients.TryAdd(chatId, chatClient))
        {
            return (chatClient, true);
        }
        else
        {
            return (_chatClients[chatId], false);
        }
    }
}