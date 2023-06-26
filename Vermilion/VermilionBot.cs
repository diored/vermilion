namespace DioRed.Vermilion;

public abstract class VermilionBot
{
    private VermilionManager? _botManager;

    protected bool NewChatsDetection { get; set; } = true;

    public VermilionManager Manager
    {
        get
        {
            return _botManager
                ?? throw new NullReferenceException("Bot manager should be set before any bot usage");
        }
        set
        {
            _botManager = value;
        }
    }

    protected ICollection<ChatId> GetAllChats()
    {
        return Manager.Chats.GetStoredChats(System);
    }

    protected ChatClient GetChatClient(ChatId chatId, Func<ChatClient> createFunc, Func<string> getTitleFunc)
    {
        ChatClient? chatClient = Manager.Chats.GetClient(chatId);

        if (chatClient is null)
        {
            ChatClient newChatClient = createFunc();

            chatClient = NewChatsDetection
                ? Manager.Chats.AddAndStoreChatClient(chatId, newChatClient, getTitleFunc())
                : Manager.Chats.AddChatClient(chatId, newChatClient);
        }

        return chatClient;
    }

    protected internal abstract BotSystem System { get; }
    protected internal abstract Task StartAsync(CancellationToken cancellationToken);
    protected internal abstract IChatWriter GetChatWriter(ChatId chatId);
    protected internal abstract Task<UserRole> GetUserRoleAsync(long userId, ChatId chatId, CancellationToken cancellationToken);
}