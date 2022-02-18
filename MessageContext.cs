using Telegram.Bot;

namespace DioRed.Vermilion;

public record MessageContext(ITelegramBotClient BotClient, IChatClient ChatClient, Broadcaster Broadcaster, CancellationToken CancellationToken);