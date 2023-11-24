using DioRed.Vermilion.Telegram.Extensions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram;

public class TelegramChatClient(Chat chat, TelegramVermilionBot bot) : ChatClient(chat.GetChatId(), bot)
{
    public Chat Chat { get; } = chat;

    public async Task DeleteMessageAsync(int messageId, CancellationToken cancellationToken)
    {
        await bot.BotClient.DeleteMessageAsync(Chat.Id, messageId, cancellationToken);
    }

    internal async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        if (message is { Text: null } or { From: not { IsBot: false } })
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
        if (bot.TelegramBot.Username is { } username)
        {
            return message.Replace("@" + username, string.Empty).Trim();
        }

        return message;
    }
}