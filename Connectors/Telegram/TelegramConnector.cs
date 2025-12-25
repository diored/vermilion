using System.Net.Sockets;

using DioRed.Vermilion.Connectors.Telegram.L10n;
using DioRed.Vermilion.Extensions;
using DioRed.Vermilion.Interaction.Content;

using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DioRed.Vermilion.Connectors.Telegram;

public class TelegramConnector : IConnector
{
    private readonly static string _version = typeof(TelegramConnector).Assembly.GetName().Version.Normalize();

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private CancellationTokenSource? _linkedCancellationTokenSource;
    private readonly TelegramBotClient _telegramBotClient;
    private readonly ILogger<TelegramConnector> _logger;
    private User? _botInfo;
    private readonly long[] _superAdmins;
    private readonly string _connectorKey;

    public event EventHandler<MessagePostedEventArgs>? MessagePosted;

    public TelegramConnector(
        TelegramConnectorOptions options,
        ILoggerFactory loggerFactory
    )
    {
        _telegramBotClient = new TelegramBotClient(options.BotToken);
        _logger = loggerFactory.CreateLogger<TelegramConnector>();

        _superAdmins = options.SuperAdmins;
        _connectorKey = options.ConnectorKey;
    }

    public string Version => _version;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Resolve bot info lazily to avoid sync-over-async deadlocks during DI construction.
        _botInfo ??= await _telegramBotClient.GetMe(cancellationToken).ConfigureAwait(false);

        _linkedCancellationTokenSource?.Dispose();
        _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _cancellationTokenSource.Token
        );

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
                else if (exception is ApiRequestException apiEx &&
                    apiEx.Message.Contains("Bad gateway"))
                {
                    _logger.LogWarning("Bad gateway (network issue)");
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
            cancellationToken: _linkedCancellationTokenSource.Token
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        _linkedCancellationTokenSource?.Cancel();
        _linkedCancellationTokenSource?.Dispose();
        _linkedCancellationTokenSource = null;

        return Task.CompletedTask;
    }

    public async Task<PostResult> PostAsync(
        long internalId,
        IContent content
    )
    {
        Func<Task>? actionFactory = content switch
        {
            TextContent c => () => SendTextAsync(internalId, c.Text),
            HtmlContent c => () => SendTextAsync(internalId, c.Html, ParseMode.Html),
            ImageBytesContent c => () => SendPhotoBytesAsync(internalId, c.Content),
            ImageStreamContent c => () => SendPhotoAsync(internalId, InputFile.FromStream(c.Stream)),
            ImageUrlContent c => () => SendPhotoAsync(internalId, InputFile.FromUri(c.Url)),
            _ => null
        };

        if (actionFactory is null)
        {
            return PostResult.ContentTypeNotSupported;
        }

        return await DoActionAsync(actionFactory, internalId);
    }

    public bool IsSuperAdmin(ChatId chatId)
    {
        return chatId.ConnectorKey == _connectorKey
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

        string messageText = _botInfo?.Username is { } username && message.Text.EndsWith("@" + username)
            ? message.Text[..^(username.Length + 1)].Trim()
            : message.Text;

        (PostResult result, UserRole senderRole) = await DoActionAsync(
            () => GetUserRoleAsync(
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
            ChatId = GetChatId(message.Chat),
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
            () => GetUserRoleAsync(
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
            ChatId = GetChatId(callbackQuery.Message.Chat),
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

    private ChatId GetChatId(Chat chat)
    {
        return new ChatId(
            _connectorKey,
            chat.Type.ToString(),
            chat.Id
        );
    }

    private async Task<PostResult> DoActionAsync(
        Func<Task> actionFactory,
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
                await actionFactory().ConfigureAwait(false);
                return PostResult.Success;
            }
            catch (Exception ex)
            {
                TelegramException tgEx = GetTelegramException(ex, internalId);
                if (tgEx is not (TelegramException.TooManyRequests or TelegramException.NetworkIssue))
                {
                    return GetPostResult(tgEx);
                }
            }
        }

        // retry limit exceeded
        return PostResult.ConnectorFailure;
    }

    private async Task<(PostResult result, T? value)> DoActionAsync<T>(
        Func<Task<T>> actionFactory,
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
                T result = await actionFactory().ConfigureAwait(false);
                return (PostResult.Success, result);
            }
            catch (Exception ex)
            {
                TelegramException tgEx = GetTelegramException(ex, internalId);
                if (tgEx is not (TelegramException.TooManyRequests or TelegramException.NetworkIssue))
                {
                    return (GetPostResult(tgEx), default);
                }
            }
        }

        // retry limit exceeded
        return (PostResult.ConnectorFailure, default);
    }

    private TelegramException GetTelegramException(
        Exception ex,
        long internalId
    )
    {
        if (ex is SocketException ||
            ex.Message.Contains("Bad gateway"))
        {
            return TelegramException.NetworkIssue;
        }

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

        _logger.LogError(
            ex,
            ExceptionMessages.MessagePostUnhandledError_0
        );

        return TelegramException.Unexpected;
    }

    private static SocketException? FindSocketException(Exception exception)
    {
        while (exception is not SocketException)
        {
            if (exception.InnerException is null)
            {
                return null;
            }

            exception = exception.InnerException;
        }

        return (SocketException)exception;
    }

    private async Task SendPhotoBytesAsync(long internalId, byte[] bytes)
    {
        await using var stream = new MemoryStream(bytes, writable: false);
        await SendPhotoAsync(internalId, InputFile.FromStream(stream)).ConfigureAwait(false);
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
            _ => PostResult.ConnectorFailure
        };
    }

    private static string GetUserName(User user)
    {
        return $"{user.FirstName} {user.LastName}".Trim();
    }
}