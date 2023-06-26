using DioRed.Vermilion.Telegram.Extensions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram;

public class TelegramChatClient : ChatClient
{
    private readonly TelegramVermilionBot _bot;

    public TelegramChatClient(Chat chat, TelegramVermilionBot bot)
        : base(chat.GetChatId(), bot)
    {
        _bot = bot;
        Chat = chat;
    }

    public Chat Chat { get; }

    public async Task DeleteMessageAsync(int messageId, CancellationToken cancellationToken)
    {
        await _bot.BotClient.DeleteMessageAsync(Chat.Id, messageId, cancellationToken);
    }

    internal async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.Text is null ||
            message.From?.IsBot != false)
        {
            return;
        }

        string messageText = TrimBotName(message.Text);
        await HandleMessageAsync(messageText, message.From.Id, message.MessageId, cancellationToken);
    }

    internal async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data is null)
        {
            return;
        }

        await HandleMessageAsync(callbackQuery.Data, callbackQuery.From.Id, 0, cancellationToken);
    }

    private string TrimBotName(string message)
    {
        if (_bot.TelegramBot.Username is { } username)
        {
            return message.Replace("@" + username, string.Empty).Trim();
        }

        return message;
    }
}