using DioRed.Vermilion.Logging;

namespace DioRed.Vermilion;

public class VermilionManager
{
    private readonly Dictionary<BotSystem, VermilionBot> _bots = new();
    private readonly VermilionConfiguration _configuration;
    private readonly IMessageHandlerBuilder _messageHandlerBuilder;

    private CancellationTokenSource _cts;

    public VermilionManager(VermilionConfiguration configuration, IChatStorage chatStorage, IMessageHandlerBuilder messageHandlerBuilder)
    {
        _configuration = configuration;
        _messageHandlerBuilder = messageHandlerBuilder;

        _cts = new CancellationTokenSource();

        Chats = new ChatManager(chatStorage);

        Logger = new MultiLogger();
        Logger.Loggers.Add(new ConsoleLogger());
    }

    public MultiLogger Logger { get; }
    public IChatManager Chats { get; }
    public bool UseCommandsCache => _configuration.UseCommandsCache;

    public IMessageHandler GetMessageHandler(MessageContext messageContext)
    {
        return _messageHandlerBuilder.BuildMessageHandler(messageContext);
    }

    public VermilionManager AddBot(VermilionBot bot)
    {
        _bots.Add(bot.System, bot);
        bot.Manager = this;
        return this;
    }

    public async Task Broadcast(Func<IChatWriter, Task> action)
    {
        await Broadcast((chat, token) => action(GetChatWriter(chat.ChatId)));
    }

    public async Task Broadcast(Func<ChatClient, CancellationToken, Task> action)
    {
        foreach (var chatClient in Chats.GetAllClients())
        {
            await action(chatClient, _cts.Token);
        }
    }

    public async Task StartAsync()
    {
        string greeting = _configuration.Greeting ?? "Vermilion bot manager started.";
        Logger.LogInfo(greeting);

        foreach ((BotSystem system, VermilionBot bot) in _bots)
        {
            await bot.StartAsync(_cts.Token);
            Logger.LogInfo($"{system} system started");
        }
    }

    public void Start()
    {
        StartAsync().GetAwaiter().GetResult();
    }

    public void Stop()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
    }

    internal IChatWriter GetChatWriter(ChatId chatId)
    {
        return _bots[chatId.System].GetChatWriter(chatId);
    }
}