using DioRed.Vermilion.Telegram.Extensions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion.Telegram;

public abstract class TelegramChatClient : IChatClient
{
    private readonly VermilionTelegramBot _bot;

    protected TelegramChatClient(Chat chat, VermilionTelegramBot bot)
    {
        Chat = chat;
        ChatId = chat.GetChatId();
        _bot = bot;
    }

    public Chat Chat { get; }
    public ChatId ChatId { get; }

    public async Task DeleteMessageAsync(int messageId, CancellationToken cancellationToken)
    {
        await _bot.BotClient.DeleteMessageAsync(ChatId.Id, messageId, cancellationToken);
    }

    protected virtual async Task<UserRole> GetUserRoleAsync(long userId, CancellationToken cancellationToken)
    {
        const long botSender = -1;

        if (userId == botSender)
        {
            return UserRole.Member;
        }

        if (userId == ChatId.Id)
        {
            return UserRole.ChatAdmin;
        }

        ChatMember chatMember = await _bot.BotClient.GetChatMemberAsync(ChatId.Id, userId, cancellationToken);

        return chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator
            ? UserRole.ChatAdmin
            : UserRole.Member;
    }

    protected string TrimBotName(string message)
    {
        return message.Replace("@" + _bot.BotUsername, string.Empty).Trim();
    }

    protected internal abstract Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
    protected internal abstract Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken);
}