using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

public class VermilionManager(
    VermilionConfiguration configuration,
    IChatStorage chatStorage,
    IMessageHandlerBuilder messageHandlerBuilder,
    ILogger<VermilionManager> logger)
{
    private readonly Dictionary<BotSystem, VermilionBot> _bots = [];
    private CancellationTokenSource _cts = new();

    public IChatManager Chats { get; } = new ChatManager(chatStorage);
    public bool UseCommandsCache => configuration.UseCommandsCache;

    public IMessageHandler GetMessageHandler(MessageContext messageContext)
    {
        return messageHandlerBuilder.BuildMessageHandler(messageContext);
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
        string greeting = configuration.Greeting ?? "Vermilion bot manager started.";
        logger.LogInformation(greeting);
        await Console.Out.WriteLineAsync(greeting);

        foreach ((BotSystem system, VermilionBot bot) in _bots)
        {
            await bot.StartAsync(_cts.Token);
            logger.LogInformation("{System} system started", system);
            await Console.Out.WriteLineAsync($"{system} system started");
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