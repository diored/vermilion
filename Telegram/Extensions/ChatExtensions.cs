using Telegram.Bot.Types;

namespace DioRed.Vermilion.Telegram.Extensions;

internal static class ChatExtensions
{
    public static ChatId GetChatId(this Chat chat)
    {
        return new ChatId(
            System: BotSystem.Telegram,
            Type: chat.Type.ToString(),
            Id: chat.Id
        );
    }
}