using System.Collections.Concurrent;

namespace DioRed.Vermilion;

/// <summary>
/// Manages runtime chat clients known to the bot.
/// </summary>
public interface IChatClientsManager
{
    /// <summary>
    /// Adds a chat client for the specified metadata.
    /// </summary>
    bool Add(ChatMetadata metadata);

    /// <summary>
    /// Gets a chat client by chat identity.
    /// </summary>
    ChatClient? Get(ChatId chatId);

    /// <summary>
    /// Gets all chat clients.
    /// </summary>
    ChatClient[] GetAll();

    /// <summary>
    /// Finds chat clients whose metadata matches the specified predicate.
    /// </summary>
    ChatClient[] Find(Func<ChatMetadata, bool> condition);

    /// <summary>
    /// Removes a chat client by chat identity.
    /// </summary>
    void Remove(ChatId chatId);

    /// <summary>
    /// Replaces the chat client stored under the specified chat identity.
    /// </summary>
    void Set(ChatId chatId, ChatClient chatClient);
}

/// <summary>
/// Default in-memory implementation of <see cref="IChatClientsManager"/>.
/// </summary>
public class ChatClientsManager : IChatClientsManager
{
    private readonly ConcurrentDictionary<ChatId, ChatClient> _chatClients = [];

    /// <inheritdoc />
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

    /// <inheritdoc />
    public ChatClient? Get(ChatId chatId)
    {
        _chatClients.TryGetValue(chatId, out ChatClient? client);
        return client;
    }

    /// <inheritdoc />
    public ChatClient[] GetAll()
    {
        return [.. _chatClients.Values];
    }

    /// <inheritdoc />
    public ChatClient[] Find(Func<ChatMetadata, bool> condition)
    {
        return [.. _chatClients.Values.Where(chat => condition(chat.Metadata))];
    }

    /// <inheritdoc />
    public void Remove(ChatId chatId)
    {
        _chatClients.Remove(chatId, out _);
    }

    /// <inheritdoc />
    public void Set(ChatId chatId, ChatClient chatClient)
    {
        _chatClients[chatId] = chatClient;
    }
}
