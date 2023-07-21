using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram;

internal class UpdateHandler : IUpdateHandler
{
    private readonly TelegramVermilionBot _bot;
    private readonly ILogger _logger;

    public UpdateHandler(TelegramVermilionBot bot, ILogger logger)
    {
        _bot = bot;
        _logger = logger;
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string exceptionType = exception switch
        {
            HttpRequestException => "HTTP",
            ApiRequestException => "API",
            RequestException => "Request",
            _ => "Unexpected"
        };

        _logger.LogError(exception, "{Type} error occurred during message polling", exceptionType);

        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => _bot.HandleMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => _bot.HandleMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => _bot.HandleCallbackQueryReceived(callbackQuery, cancellationToken),
            _ => _bot.HandleOtherUpdateReceived(update, cancellationToken)
        };

        await handler;
    }
}