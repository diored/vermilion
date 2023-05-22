using Telegram.Bot;

namespace DioRed.Vermilion;

public record MessageContext(
    ITelegramBotClient BotClient,
    IChatClient ChatClient,
    UserRole Role,
    Broadcaster Broadcaster,
    int MessageId,
    CancellationToken CancellationToken);