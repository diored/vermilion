using DioRed.Vermilion.Telegram;
using DioRed.Vermilion.Telegram.Extensions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion;

public abstract class VermilionTelegramBot : VermilionBot
{
    private readonly IChatStorage _chatStorage;

    public VermilionTelegramBot(TelegramBotConfiguration configuration, IChatStorage chatStorage)
        : base(chatStorage)
    {
        _chatStorage = chatStorage;

        BotClient = new TelegramBotClient(configuration.BotToken);

        var bot = BotClient.GetMeAsync(CancellationToken).GetAwaiter().GetResult();
        BotId = bot.Id;
        BotUsername = bot.Username!;
    }

    public ITelegramBotClient BotClient { get; }
    public long BotId { get; }
    public string BotUsername { get; }

    protected override void StartInternal()
    {
        BotClient.StartReceiving(new UpdateHandler(this), cancellationToken: CancellationToken);
    }

    protected override void ReconnectToChat(ChatId chatId)
    {
        try
        {
            Chat chat = BotClient.GetChatAsync(chatId.Id).GetAwaiter().GetResult();
            _ = GetChatClient(chat);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Cannot connect to chat #{chatId}: {ex.Message}");
            if (ex.Message.Contains("kicked") || ex.Message.Contains("blocked"))
            {
                _chatStorage.RemoveChat(chatId);
            }
        }
    }

    protected internal TelegramChatClient GetChatClient(Chat chat)
    {
        return GetOrCreateChatClient(
            chat.GetChatId(),
            () => CreateChatClient(chat),
            () => chat.Type == ChatType.Private ? $"{chat.FirstName} {chat.LastName}".Trim() : chat.Title ?? string.Empty);
    }

    protected override IChatWriter GetChatWriter(ChatId chatId)
    {
        return new TelegramChatWriter(BotClient, chatId.Id);
    }

    protected abstract TelegramChatClient CreateChatClient(Chat chat);

    internal async Task HandleMessageReceived(Message message, CancellationToken cancellationToken)
    {
        var chatClient = GetChatClient(message.Chat);
        await chatClient.HandleMessageAsync(message, cancellationToken);
    }

    internal async Task HandleCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        Message? message = callbackQuery.Message;
        if (message is null) // too old
        {
            return;
        }

        var chatClient = GetChatClient(message.Chat);
        await chatClient.HandleCallbackQueryAsync(callbackQuery, cancellationToken);
    }

    protected internal virtual Task HandleOtherUpdateReceived(Update update, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}