using System.Collections.Concurrent;

namespace DioRed.Vermilion;

public abstract class VermilionBot
{
    private readonly ConcurrentDictionary<ChatId, IChatClient> _chatClients = new();

    private readonly IChatStorage _chatStorage;
    private readonly CancellationTokenSource _cts;

    private bool _newChatsDetection;

    protected VermilionBot(IChatStorage chatStorage)
    {
        _cts = new CancellationTokenSource();
        _chatStorage = chatStorage;

        _newChatsDetection = true;

        Logger = new MultiLogger();
        Logger.Loggers.Add(new ConsoleLogger());
    }

    public MultiLogger Logger { get; }

    protected CancellationToken CancellationToken => _cts.Token;

    public async Task Broadcast(Func<IChatClient, CancellationToken, Task> action)
    {
        foreach (var chatClient in _chatClients.Values)
        {
            await action(chatClient, _cts.Token);
        }
    }

    public async Task Broadcast(Func<IChatWriter, Task> action)
    {
        await Broadcast((chat, token) => action(GetChatWriter(chat.ChatId)));
    }

    public void Start()
    {
        ReconnectToChats();
        StartInternal();
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    protected TChatClient GetOrCreateChatClient<TChatClient>(ChatId chatId, Func<TChatClient> createChatClientFunc, Func<string> getChatTitleFunc)
        where TChatClient : IChatClient
    {
        if (_chatClients.TryGetValue(chatId, out IChatClient? chatClient))
        {
            return (TChatClient)chatClient;
        }

        TChatClient newChatClient = createChatClientFunc();

        if (_chatClients.TryAdd(chatId, newChatClient))
        {
            if (_newChatsDetection)
            {
                _chatStorage.AddChat(chatId, getChatTitleFunc());
            }

            return newChatClient;
        }
        else
        {
            return (TChatClient)_chatClients[chatId];
        }
    }

    private void ReconnectToChats()
    {
        ICollection<ChatId> chatIds = _chatStorage.GetChats();

        try
        {
            _newChatsDetection = false;
            foreach (ChatId chatId in chatIds)
            {
                ReconnectToChat(chatId);
            }
        }
        finally
        {
            _newChatsDetection = true;
        }
    }

    protected abstract void StartInternal();
    protected abstract void ReconnectToChat(ChatId chatId);
    protected internal abstract IChatWriter GetChatWriter(ChatId chatId);
}
