using Telegram.Bot.Types;

namespace DioRed.Vermilion;

public interface IChatClient
{
    Chat Chat { get; }
    Task HandleCallbackQueryAsync(Bot bot, CallbackQuery callbackQuery, CancellationToken cancellationToken);
    Task HandleMessageAsync(Bot bot, Message message, CancellationToken cancellationToken);
}