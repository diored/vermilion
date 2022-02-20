using System.Collections.Concurrent;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion;

public abstract class Bot : IUpdateHandler
{
    private readonly ConcurrentDictionary<long, IChatClient> _chatClients = new();
    private readonly HashSet<ILogger> _loggers = new();

    protected Bot(BotConfiguration configuration, CancellationToken cancellationToken)
    {
        BotClient = new TelegramBotClient(configuration.BotToken);
        CancellationToken = cancellationToken;
        Broadcaster = new Broadcaster(this);
    }

    public ITelegramBotClient BotClient { get; }

    protected CancellationToken CancellationToken { get; private set; }

    protected Broadcaster Broadcaster { get; }

    public void AddLogger(ILogger logger)
    {
        _loggers.Add(logger);
    }

    public void RemoveLogger(ILogger logger)
    {
        _loggers.Remove(logger);
    }

    public async Task ConnectToChatAsync(long chatId)
    {
        try
        {
            Chat chat = await BotClient.GetChatAsync(chatId);
            _ = GetChatClient(chat);
        }
        catch (Exception ex)
        {
            LogError($"Cannot connect to chat #{chatId}: {ex.Message}");

            throw;
        }
    }

    public virtual void StartReceiving()
    {
        BotClient.StartReceiving(this, cancellationToken: CancellationToken);
    }

    public async Task Broadcast(Func<IChatClient, CancellationToken, Task> action)
    {
        foreach (var chatClient in _chatClients.Values)
        {
            await action(chatClient, CancellationToken);
        }
    }

    public async Task Broadcast(Func<IChatWriter, Task> action)
    {
        await Broadcast((chat, token) => action(new ChatWriter(BotClient, chat.Chat.Id)));
    }

    public void LogError(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.LogError(message);
        }
    }

    public void LogInfo(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.LogInfo(message);
        }
    }

    protected virtual void OnChatClientAdded(Chat chat)
    {
    }

    private IChatClient GetChatClient(Chat chat)
    {
        if (_chatClients.TryGetValue(chat.Id, out IChatClient? chatClient))
        {
            return chatClient;
        }

        chatClient = CreateChatClient(chat);

        if (_chatClients.TryAdd(chat.Id, chatClient))
        {
            OnChatClientAdded(chat);
        }
        else
        {
            chatClient = _chatClients[chat.Id];
        }

        return chatClient;
    }

    protected abstract IChatClient CreateChatClient(Chat chat);

    async Task IUpdateHandler.HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        IChatClient chatClient;

        switch (update.Type)
        {
            case UpdateType.Message:
                chatClient = GetChatClient(update.Message!.Chat);
                await chatClient.HandleMessageAsync(this, update.Message, cancellationToken);
                break;

            case UpdateType.CallbackQuery:
                chatClient = GetChatClient(update.CallbackQuery!.Message!.Chat);
                await chatClient.HandleCallbackQueryAsync(this, update.CallbackQuery, cancellationToken);
                break;

            default:
                // just ignoring everything else
                break;
        }
    }

    Task IUpdateHandler.HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string message = exception switch
        {
            HttpRequestException httpEx => $"HTTP request error: {httpEx.Message}",
            ApiRequestException apiEx => $"API request error [{apiEx.ErrorCode}]: {apiEx.Message}",
            RequestException reqEx => $"Request exception: {reqEx.Message}",
            _ => $"Error: {exception}"
        };

        LogError(message);

        return Task.CompletedTask;
    }
}
