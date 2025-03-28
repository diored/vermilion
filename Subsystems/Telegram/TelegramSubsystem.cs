using System.Net.Sockets;

using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Subsystems.Telegram.L10n;

using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion.Subsystems.Telegram;

public class TelegramSubsystem : ISubsystem
{
    private readonly static string _version = typeof(TelegramSubsystem).Assembly.GetName().Version.Normalize();

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

        _botInfo = _telegramBotClient.GetMe().GetAwaiter().GetResult();
        _superAdmins = options.SuperAdmins;
    }

    public string Version => _version;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _telegramBotClient.StartReceiving(
            updateHandler: (_, update, token) => update switch
            {
                { Message: { } message } => HandleMessageReceived(message, token),
                { EditedMessage: { } message } => HandleMessageReceived(message, token),
                { CallbackQuery: { } callbackQuery } => HandleCallbackQueryReceived(callbackQuery, token),
                _ => Task.CompletedTask
            },
            errorHandler: (_, exception, _) =>
            {
                string exceptionType = exception switch
                {
                    HttpRequestException => "HTTP",
                    ApiRequestException => "API",
                    RequestException => "Request",
                    _ => "Unexpected"
                };

                if (FindSocketException(exception) is { } socketException)
                {
                    _logger.LogWarning("Socket exception: {Message}", socketException.Message);
                }
                else
                {
                    _logger.LogError(
                        exception,
                        LogMessages.MessagePollingError_1,
                        exceptionType
                    );
                }

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

        return await DoActionAsync(action, internalId);
    }

    public bool IsSuperAdmin(ChatId chatId)
    {
        return chatId.System == TelegramDefaults.System
            && _superAdmins.Contains(chatId.Id);
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

        (PostResult result, UserRole senderRole) = await DoActionAsync(
            GetUserRoleAsync(
                message.From.Id,
                message.Chat,
                cancellationToken
            ),
            message.Chat.Id
        );

        if (result != PostResult.Success)
        {
            _logger.LogInformation("Cannot get sender role. Message ignored");
            return;
        }

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
            SenderRole = senderRole,
            SenderName = GetUserName(message.From)
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

        (PostResult result, UserRole senderRole) = await DoActionAsync(
            GetUserRoleAsync(
                callbackQuery.From.Id,
                callbackQuery.Message.Chat,
                cancellationToken
            ),
            callbackQuery.Message.Chat.Id
        );

        if (result != PostResult.Success)
        {
            _logger.LogInformation("Cannot get sender role. Message ignored");
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
            SenderRole = senderRole,
            SenderName = GetUserName(callbackQuery.From)
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
            ChatMember chatMember = await _telegramBotClient.GetChatMember(
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
        ParseMode parseMode = ParseMode.None
    )
    {
        _ = await _telegramBotClient.SendMessage(
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
        _ = await _telegramBotClient.SendPhoto(
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

    private async Task<PostResult> DoActionAsync(
        Task action,
        long internalId
    )
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
            catch (Exception ex)
            {
                TelegramException tgEx = GetTelegramException(ex, internalId);
                if (tgEx is not (TelegramException.TooManyRequests or TelegramException.SocketException))
                {
                    return GetPostResult(tgEx);
                }
            }
        }

        // retry limit exceeded
        return PostResult.SubsystemFailure;
    }

    private async Task<(PostResult result, T? value)> DoActionAsync<T>(
        Task<T> action,
        long internalId
    )
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
                T result = await action;
                return (PostResult.Success, result);
            }
            catch (Exception ex)
            {
                TelegramException tgEx = GetTelegramException(ex, internalId);
                if (tgEx is not (TelegramException.TooManyRequests or TelegramException.SocketException))
                {
                    return (GetPostResult(tgEx), default);
                }
            }
        }

        // retry limit exceeded
        return (PostResult.SubsystemFailure, default);
    }

    private TelegramException GetTelegramException(
        Exception ex,
        long internalId
    )
    {
        if (ex is ApiRequestException apiEx)
        {
            if (ex.Message.Contains("blocked") ||
                ex.Message.Contains("kicked") ||
                ex.Message.Contains("deactivated"))
            {
                _logger.LogInformation(
                    LogMessages.ChatBlocked_2,
                    internalId,
                    ex.Message
                );

                return TelegramException.BotBlocked;
            }

            if (ex.Message.Contains("not enough rights"))
            {
                _logger.LogInformation(
                    LogMessages.NotEnoughRights_1,
                    internalId
                );
                return TelegramException.NotEnoughRights;
            }

            if (ex.Message.Contains("group chat was upgraded to a supergroup chat"))
            {
                _logger.LogInformation(
                    LogMessages.GroupUpgradedToSuperGroup_2,
                    internalId,
                    apiEx.Parameters?.MigrateToChatId ?? 0
                );
                return TelegramException.GroupUpgraded;
            }

            if (ex.Message.Contains("chat not found") ||
                ex.Message.Contains("TOPIC_CLOSED"))
            {
                _logger.LogInformation(
                    LogMessages.ChatNotFound_1,
                    internalId
                );
                return TelegramException.ChatNotFound;
            }

            if (ex.Message.Contains("Too many requests"))
            {
                return TelegramException.TooManyRequests;
            }
        }

        if (ex is SocketException)
        {
            return TelegramException.SocketException;
        }

        _logger.LogError(
            ex,
            ExceptionMessages.MessagePostUnhandledError_0
        );
        return TelegramException.Unexpected;
    }

    private static SocketException? FindSocketException(Exception exception)
    {
        if (exception is SocketException socketException)
        {
            return socketException;
        }

        if (exception?.InnerException is null)
        {
            return null;
        }

        return FindSocketException(exception.InnerException);
    }

    private static PostResult GetPostResult(TelegramException tgEx)
    {
        return tgEx switch
        {
            TelegramException.Unexpected => PostResult.UnexpectedException,
            TelegramException.BotBlocked or
            TelegramException.ChatNotFound or
            TelegramException.NotEnoughRights or
            TelegramException.GroupUpgraded => PostResult.ChatAccessDenied,
            _ => PostResult.SubsystemFailure
        };
    }

    private static string GetUserName(User user)
    {
        return $"{user.FirstName} {user.LastName}".Trim();
    }
}