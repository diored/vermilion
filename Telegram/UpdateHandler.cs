using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram;

internal class UpdateHandler(TelegramVermilionBot bot, ILogger logger) : IUpdateHandler
{
    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string exceptionType = exception switch
        {
            HttpRequestException => "HTTP",
            ApiRequestException => "API",
            RequestException => "Request",
            _ => "Unexpected"
        };

        logger.LogError(exception, "{Type} error occurred during message polling", exceptionType);

        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => bot.HandleMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => bot.HandleMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => bot.HandleCallbackQueryReceived(callbackQuery, cancellationToken),
            _ => bot.HandleOtherUpdateReceived(update, cancellationToken)
        };

        await handler;
    }
}