using Telegram.Bot;

namespace DioRed.Vermilion;

public record MessageContext(
    ITelegramBotClient BotClient,
    IChatClient ChatClient,
    UserRole Role,
    Broadcaster Broadcaster,
    CancellationToken CancellationToken);