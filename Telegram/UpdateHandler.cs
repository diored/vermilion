using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram;

internal class UpdateHandler : IUpdateHandler
{
    private readonly TelegramVermilionBot _bot;

    public UpdateHandler(TelegramVermilionBot bot)
    {
        _bot = bot;
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string message = exception switch
        {
            HttpRequestException httpEx => $"HTTP request error: {httpEx.Message}",
            ApiRequestException apiEx => $"API request error [{apiEx.ErrorCode}]: {apiEx.Message}",
            RequestException reqEx => $"Request exception: {reqEx.Message}",
            _ => $"Error: {exception}"
        };

        _bot.Manager.Logger.LogError(message);

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
