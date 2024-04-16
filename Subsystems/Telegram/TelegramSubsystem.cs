using DioRed.Vermilion.Interaction.Content;

using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion.Subsystems.Telegram;

public class TelegramSubsystem : ISubsystem
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TelegramBotClient _telegramBotClient;
    private readonly ILogger<TelegramSubsystem> _logger;
    private readonly User _botInfo;
    private readonly long[] _superAdmins;

    public event EventHandler<MessagePostedEventArgs>? MessagePosted;

    public TelegramSubsystem(
        TelegramSubsystemOptions options,
        ILoggerFactory loggerFactory
    )
    {
        _telegramBotClient = new TelegramBotClient(options.BotToken);
        _logger = loggerFactory.CreateLogger<TelegramSubsystem>();

        _botInfo = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
        _superAdmins = options.SuperAdmins;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _telegramBotClient.StartReceiving(
            updateHandler: (_, update, cancellationToken) => update switch
            {
                { Message: { } message } => HandleMessageReceived(message, cancellationToken),
                { EditedMessage: { } message } => HandleMessageReceived(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => HandleCallbackQueryReceived(callbackQuery, cancellationToken),
                _ => Task.CompletedTask
            },
            pollingErrorHandler: (_, exception, _) =>
            {
                string exceptionType = exception switch
                {
                    HttpRequestException => "HTTP",
                    ApiRequestException => "API",
                    RequestException => "Request",
                    _ => "Unexpected"
                };

                _logger.LogError(exception, "{Type} error occurred during message polling", exceptionType);

                return Task.CompletedTask;
            },
            cancellationToken: _cancellationTokenSource.Token
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();

        return Task.CompletedTask;
    }

    public async Task<PostResult> PostAsync(
        long internalId,
        IContent content
    )
    {
        Task? action = content switch
        {
            TextContent c => SendTextAsync(internalId, c.Text),
            HtmlContent c => SendTextAsync(internalId, c.Html, ParseMode.Html),
            ImageBytesContent c => SendPhotoAsync(internalId, InputFile.FromStream(new MemoryStream(c.Content))),
            ImageStreamContent c => SendPhotoAsync(internalId, InputFile.FromStream(c.Stream)),
            ImageUrlContent c => SendPhotoAsync(internalId, InputFile.FromUri(c.Url)),
            _ => null
        };

        if (action is null)
        {
            return PostResult.ContentTypeNotSupported;
        }

        try
        {
            const int maxRetryCount = 5;
            for (int i = 0; i < maxRetryCount; i++)
            {
                if (i != 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5 + (i + 1)));
                }

                try
                {
                    await action;
                    return PostResult.Success;
                }
                catch (ApiRequestException ex) when (ex.Message.Contains("Too many requests"))
                {
                    // let's try again
                }
            }

            return PostResult.SubsystemFailure;
        }
        catch (Exception ex) when (
            ex.Message.Contains("blocked") ||
            ex.Message.Contains("kicked") ||
            ex.Message.Contains("deactivated"))
        {
            _logger.LogInformation(
                "Chat {ChatId} was probably blocked. Message: {Message}",
                internalId,
                ex.Message
            );
            return PostResult.ChatAccessDenied;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred during message posting"
            );
        }
        return PostResult.UnexpectedException;
    }

    protected virtual void OnMessagePosted(MessagePostedEventArgs e)
    {
        MessagePosted?.Invoke(this, e);
    }

    private async Task HandleMessageReceived(
        Message message,
        CancellationToken cancellationToken
    )
    {
        if (message is not { Type: MessageType.Text } or { Text: null } or { From: not { IsBot: false } })
        {
            return;
        }

        string messageText = _botInfo.Username is { } username && message.Text.EndsWith("@" + username)
            ? message.Text[..^(username.Length + 1)].Trim()
            : message.Text;

        OnMessagePosted(new MessagePostedEventArgs
        {
            ChatId = new ChatId(
                TelegramDefaults.System,
                message.Chat.Type.ToString(),
                message.Chat.Id
            ),
            ChatTitle = GetChatTitle(message.Chat),
            Message = messageText,
            MessageId = message.MessageId,
            SenderId = message.From.Id,
            SenderRole = await GetUserRoleAsync(
                message.From.Id,
                message.Chat,
                cancellationToken
            )
        });
    }

    private async Task HandleCallbackQueryReceived(
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        if (callbackQuery is { Data: null } or { Message: null })
        {
            return;
        }

        OnMessagePosted(new MessagePostedEventArgs
        {
            ChatId = new ChatId(
                TelegramDefaults.System,
                callbackQuery.Message.Chat.Type.ToString(),
                callbackQuery.Message.Chat.Id
            ),
            ChatTitle = GetChatTitle(callbackQuery.Message.Chat),
            Message = callbackQuery.Data,
            MessageId = callbackQuery.Message.MessageId,
            SenderId = callbackQuery.From.Id,
            SenderRole = await GetUserRoleAsync(
                callbackQuery.From.Id,
                callbackQuery.Message.Chat,
                cancellationToken
            )
        });
    }

    private async Task<UserRole> GetUserRoleAsync(
        long userId,
        Chat chat,
        CancellationToken cancellationToken
    )
    {
        if (userId == -1)
        {
            return UserRole.Bot;
        }

        UserRole userRole = UserRole.Member;

        if (chat.Type == ChatType.Private &&
            userId == chat.Id)
        {
            userRole |= UserRole.ChatAdmin;

            if (_superAdmins.Contains(userId))
            {
                userRole |= UserRole.SuperAdmin;
            }
        }
        else
        {
            ChatMember chatMember = await _telegramBotClient.GetChatMemberAsync(
                chat.Id,
                userId,
                cancellationToken
            );

            if (chatMember.Status is ChatMemberStatus.Administrator or ChatMemberStatus.Creator)
            {
                userRole |= UserRole.ChatAdmin;
            }
        }

        return userRole;
    }

    private async Task SendTextAsync(
        long internalId,
        string text,
        ParseMode? parseMode = null
    )
    {
        _ = await _telegramBotClient.SendTextMessageAsync(
            internalId,
            text,
            parseMode: parseMode
        );
    }

    private async Task SendPhotoAsync(
        long internalId,
        InputFile file
    )
    {
        _ = await _telegramBotClient.SendPhotoAsync(
            internalId,
            file
        );
    }

    private static string GetChatTitle(Chat chat)
    {
        return chat.Type == ChatType.Private
            ? $"{chat.FirstName} {chat.LastName}".Trim()
            : chat.Title ?? "";
    }
}