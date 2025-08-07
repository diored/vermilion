using System.Collections.Concurrent;

namespace DioRed.Vermilion;

public interface IChatClientsManager
{
    bool Add(ChatMetadata metadata);
    ChatClient? Get(ChatId chatId);
    ChatClient[] GetAll();
    ChatClient[] Find(Func<ChatMetadata, bool> condition);
    void Remove(ChatId chatId);
    void Set(ChatId chatId, ChatClient chatClient);
}

public class ChatClientsManager : IChatClientsManager
{
    private readonly ConcurrentDictionary<ChatId, ChatClient> _chatClients = [];

    public bool Add(ChatMetadata metadata)
    {
        return _chatClients.TryAdd(
            metadata.ChatId,
            new ChatClient
            {
                Metadata = metadata
            }
        );
    }

    public ChatClient? Get(ChatId chatId)
    {
        _chatClients.TryGetValue(chatId, out ChatClient? client);
        return client;
    }

    public ChatClient[] GetAll()
    {
        return [.. _chatClients.Values];
    }

    public ChatClient[] Find(Func<ChatMetadata, bool> condition)
    {
        return [.. _chatClients.Values.Where(chat => condition(chat.Metadata))];
    }

    public void Remove(ChatId chatId)
    {
        _chatClients.Remove(chatId, out _);
    }

    public void Set(ChatId chatId, ChatClient chatClient)
    {
        _chatClients[chatId] = chatClient;
    }
}