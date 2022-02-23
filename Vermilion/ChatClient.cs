using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion;

public abstract class ChatClient : IChatClient
{
    protected ChatClient(Chat chat)
    {
        Chat = chat;
    }

    public Chat Chat { get; }

    public abstract Task HandleCallbackQueryAsync(Bot bot, CallbackQuery callbackQuery, CancellationToken cancellationToken);
    public abstract Task HandleMessageAsync(Bot bot, Message message, CancellationToken cancellationToken);

    protected virtual async Task<UserRole> GetUserRoleAsync(ITelegramBotClient botClient, long userId, CancellationToken cancellationToken)
    {
        if (userId == Bot.BotSenderId)
        {
            return UserRole.AnyUser;
        }

        if (Chat.Id == userId)
        {
            return UserRole.ChatAdmin;
        }

        var chatMember = await botClient.GetChatMemberAsync(Chat.Id, userId, cancellationToken);

        return chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator
            ? UserRole.ChatAdmin
            : UserRole.AnyUser;
    }

    protected static async Task<string> GetBotNameAsync(ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        return (await botClient.GetMeAsync(cancellationToken)).Username!;
    }

    protected static string TrimBotName(string message, string botName)
    {
        Index start = message.StartsWith(botName + " ") ? botName.Length + 1 : 0;
        Index end = message.EndsWith(botName) ? ^botName.Length : ^0;

        return message[start..end];
    }
}
