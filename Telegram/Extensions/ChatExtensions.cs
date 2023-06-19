using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram.Extensions;

internal static class ChatExtensions
{
    public static ChatId GetChatId(this Chat chat)
    {
        return new ChatId(
            Type: "Telegram" + chat.Type,
            Id: chat.Id
        );
    }
}
