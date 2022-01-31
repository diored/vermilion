using Telegram.Bot.Types;

namespace DioRed.Vermilion;

public record BotConfiguration(string Token, Func<Chat, IChatClient> ChatClientConstructor);