using DioRed.Vermilion.Telegram;
using DioRed.Vermilion.Telegram.Extensions;

using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion;

public class TelegramVermilionBot : VermilionBot
{
    private readonly long? _superAdminId;
    private readonly ILogger _logger;

    public TelegramVermilionBot(TelegramBotConfiguration configuration, ILogger<TelegramVermilionBot> logger)
    {
        BotClient = new TelegramBotClient(configuration.BotToken);
        TelegramBot = BotClient.GetMeAsync().GetAwaiter().GetResult();
        _superAdminId = configuration.SuperAdminId;
        _logger = logger;
    }

    public ITelegramBotClient BotClient { get; }
    public User TelegramBot { get; }

    protected sealed override BotSystem System => BotSystem.Telegram;

    protected override async Task StartAsync(CancellationToken cancellationToken)
    {
        await ReconnectToChatsAsync(cancellationToken);
        BotClient.StartReceiving(new UpdateHandler(this, _logger), cancellationToken: cancellationToken);
    }

    private async Task ReconnectToChatsAsync(CancellationToken cancellationToken)
    {
        ICollection<ChatId> chatIds = GetAllChats();

        try
        {
            NewChatsDetection = false;
            foreach (ChatId chatId in chatIds)
            {
                await ReconnectToChatAsync(chatId, cancellationToken);
            }
        }
        finally
        {
            NewChatsDetection = true;
        }
    }

    private async Task ReconnectToChatAsync(ChatId chatId, CancellationToken cancellationToken)
    {
        try
        {
            Chat chat = await BotClient.GetChatAsync(chatId.Id, cancellationToken);
            _ = GetTelegramChatClient(chat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot connect to chat ({ChatId})", chatId);
            if (ex.Message.Contains("kicked") || ex.Message.Contains("blocked"))
            {
                Manager.Chats.Remove(chatId);
            }
        }
    }

    protected override IChatWriter GetChatWriter(ChatId chatId)
    {
        return new TelegramChatWriter(BotClient, chatId.Id);
    }

    internal async Task HandleMessageReceived(Message message, CancellationToken cancellationToken)
    {
        var chatClient = GetTelegramChatClient(message.Chat);
        await chatClient.HandleMessageAsync(message, cancellationToken);
    }

    internal async Task HandleCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        Message? message = callbackQuery.Message;
        if (message is null) // too old
        {
            return;
        }

        var chatClient = GetTelegramChatClient(message.Chat);
        await chatClient.HandleCallbackQueryAsync(callbackQuery, cancellationToken);
    }

    protected internal virtual Task HandleOtherUpdateReceived(Update update, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override async Task<UserRole> GetUserRoleAsync(long userId, ChatId chatId, CancellationToken cancellationToken)
    {
        const long botSender = -1;

        if (userId == botSender)
        {
	        return UserRole.Bot;
        }

        if (chatId.Type == ChatType.Private.ToString() && userId == chatId.Id)
        {
            UserRole userRole = UserRole.ChatAdmin;
            if (userId == _superAdminId)
            {
                userRole |= UserRole.SuperAdmin;
            }

            return userRole;
        }

        ChatMember chatMember = await BotClient.GetChatMemberAsync(chatId.Id, userId, cancellationToken);

        return chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator
            ? UserRole.ChatAdmin
            : UserRole.Member;
    }

    private TelegramChatClient GetTelegramChatClient(Chat chat)
    {
	    ChatClient Create() => new TelegramChatClient(chat, this);

	    string GetTitle() => chat.Type == ChatType.Private
		    ? $"{chat.FirstName} {chat.LastName}".Trim()
		    : chat.Title ?? string.Empty;

	    return (TelegramChatClient)GetChatClient(chat.GetChatId(), Create, GetTitle);
    }
}