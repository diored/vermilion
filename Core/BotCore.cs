using System.Collections.Concurrent;

using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;
using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Interaction.Receivers;
using DioRed.Vermilion.L10n;
using DioRed.Vermilion.Subsystems;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ChatProperties = System.Collections.Generic.Dictionary<string, object?>;

namespace DioRed.Vermilion;

public class BotCore(
    IChatStorage chatStorage,
    IEnumerable<KeyValuePair<string, ISubsystem>> subsystems,
    IEnumerable<ICommandHandler> commandHandlers,
    BotOptions options,
    ILogger<BotCore> logger
) : IHostedService
{
    public static readonly object _lock = new();

    private readonly Dictionary<string, ISubsystem> _subsystems = new(subsystems);
    private readonly ICommandHandler[] _commandHandlers = [.. commandHandlers];
    private readonly ConcurrentDictionary<ChatId, ChatProperties> _chatClients = [];

    private BotCoreState _state = BotCoreState.NotInitialized;

    public static string Version { get; } = typeof(BotCore).Assembly.GetName().Version?.ToString() switch
    {
        null => "0.0",
        var v when v.EndsWith(".0.0") => v[..^4],
        var v when v.EndsWith(".0") => v[..^2],
        var v => v
    };

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_state is BotCoreState.NotInitialized)
            {
                Initialize();
            }

            if (_state is not (BotCoreState.Initialized or BotCoreState.Stopped))
            {
                throw new InvalidOperationException(
                    string.Format(
                        ExceptionMessages.CannotStartBotCoreInState_1,
                        _state
                    )
                );
            }

            _state = BotCoreState.Starting;
        }

        bool atLeastOneSubsystemStarted = false;

        foreach (var subsystem in _subsystems)
        {
            try
            {
                await subsystem.Value.StartAsync(cancellationToken);

                atLeastOneSubsystemStarted = true;

                logger.LogInformation(
                    LogMessages.SubsystemStarted_1,
                    subsystem.Key
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    LogMessages.SubsystemNotStarted_1,
                    subsystem.Key
                );
            }
        }

        _state = atLeastOneSubsystemStarted
            ? BotCoreState.Started
            : BotCoreState.Stopped;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_state is BotCoreState.Stopped)
            {
                logger.LogInformation(
                    LogMessages.BotCoreAlreadyStopped_0
                );
                return;
            }

            if (_state is not BotCoreState.Started)
            {
                throw new InvalidOperationException(
                    string.Format(
                        ExceptionMessages.CannotStopBotCoreInState_1,
                        _state
                    )
                );
            }

            _state = BotCoreState.Stopping;
        }

        bool atLeastOneSubsystemFailedToStop = false;

        foreach (var subsystem in _subsystems)
        {
            try
            {
                await subsystem.Value.StopAsync(cancellationToken);

                logger.LogInformation(
                    LogMessages.SubsystemStopped_1,
                    subsystem.Key
                );
            }
            catch (Exception ex)
            {
                atLeastOneSubsystemFailedToStop = true;

                logger.LogError(
                    ex,
                    LogMessages.SubsystemNotStopped_1,
                    subsystem.Key
                );
            }
        }

        _state = atLeastOneSubsystemFailedToStop
            ? BotCoreState.Started
            : BotCoreState.Stopped;
    }

    public Task PostAsync(Receiver receiver, string text)
    {
        return PostAsync(
            receiver,
            new TextContent
            {
                Text = text
            }
        );
    }

    public Task PostAsync(Receiver receiver, IContent content)
    {
        return PostAsync(
            receiver,
            _ => content
        );
    }

    public Task PostAsync(Receiver receiver, Func<ChatId, string> text)
    {
        return PostAsync(
            receiver,
            chatId => new TextContent
            {
                Text = text(chatId)
            }
        );
    }

    public Task PostAsync(Receiver receiver, Func<ChatId, IContent> contentBuilder)
    {
        return PostAsync(
            receiver,
            chatId => Task.FromResult(
                contentBuilder(chatId)
            )
        );
    }

    public async Task PostAsync(Receiver receiver, Func<ChatId, Task<IContent>> contentBuilder)
    {
        ChatId[] chats = GetChats(receiver);

        foreach (var chat in chats)
        {
            IContent content = await contentBuilder(chat);

            PostResult postResult = await _subsystems[chat.System].PostAsync(
                chat.Id,
                content
            );

            await ProcessResultAsync(postResult, chat, content);
        }
    }

    private void Initialize()
    {
        lock (_lock)
        {
            if (_state != BotCoreState.NotInitialized)
            {
                throw new InvalidOperationException(
                    ExceptionMessages.BotCoreAlreadyInitialized_0
                );
            }

            _state = BotCoreState.Initializing;
        }

        try
        {
            ChatId[] chats = chatStorage.GetChatsAsync().GetAwaiter().GetResult();

            foreach (ChatId chatId in chats)
            {
                _ = _chatClients.TryAdd(
                    chatId,
                    []
                );
            }

            foreach (ISubsystem subsystem in _subsystems.Values)
            {
                subsystem.MessagePosted += (_, args) => OnMessagePosted(args);
            }

            if (options.ShowCoreVersion)
            {
                logger.LogInformation(
                    LogMessages.CoreVersionInfo_1,
                    Version
                );
            }

            if (options.Greeting is not null)
            {
                logger.LogInformation(
                    LogMessages.CustomGreeting_1,
                    options.Greeting
                );
            }

            _state = BotCoreState.Initialized;
        }
        catch
        {
            _state = BotCoreState.NotInitialized;
            throw;
        }
    }

    private ChatId[] GetChats(Receiver receiver) => receiver switch
    {
        SingleChatReceiver single => [single.ChatId],
        BroadcastReceiver broadcast => [.. _chatClients.Select(x => x.Key).Where(broadcast.Filter)],
        EveryoneReceiver => [.. _chatClients.Select(x => x.Key)],
        _ => throw new ArgumentOutOfRangeException(nameof(receiver), receiver, null)
    };

    private async Task ProcessResultAsync(PostResult postResult, ChatId chatId, IContent content)
    {
        switch (postResult)
        {
            case PostResult.Success:
                // do nothing
                break;

            case PostResult.ContentTypeNotSupported:
                logger.LogInformation(
                    LogMessages.UnsupportedContent_2,
                    content.GetType().Name,
                    chatId.System
                );
                break;

            case PostResult.ChatAccessDenied:
                logger.LogInformation(
                    LogMessages.AccessDenied_1,
                    chatId
                );

                try
                {
                    await chatStorage.RemoveChatAsync(chatId);
                    _ = _chatClients.Remove(chatId, out _);
                    logger.LogInformation(
                        Events.ChatRemoved,
                        LogMessages.ChatRemoved_1,
                        chatId
                    );
                }
                catch
                {
                    logger.LogWarning(
                        Events.ChatRemoveFailure,
                        LogMessages.ChatRemoveFailure_1,
                        chatId
                    );
                    throw;
                }
                break;

            case PostResult.SubsystemFailure:
                logger.LogInformation(
                    LogMessages.MessageDeliveryFailed_1,
                    chatId
                );
                break;

            case PostResult.UnexpectedException:
                logger.LogInformation(
                    LogMessages.UnexpectedException_1,
                    chatId
                );
                break;

            default:
                throw new InvalidOperationException(
                    string.Format(
                        ExceptionMessages.UnexpectedPostResult_1,
                        postResult
                    )
                );
        }
    }

    private async void OnMessagePosted(MessagePostedEventArgs args)
    {
        try
        {
            ChatContext chatContext = await BuildChatContextAsync();
            (string command, string? tail) = SplitMessage();
            ICommandHandler[] handlers = FindMatchedHandlers(command, tail is not null);

            if (handlers.Length == 0)
            {
                return;
            }

            MessageContext messageContext = BuildMessageContext(command, tail);

            MessageHandlingContext context = new()
            {
                Chat = chatContext,
                Message = messageContext,
                Sender = new SenderContext
                {
                    Id = args.SenderId,
                    Role = args.SenderRole,
                    Name = args.SenderName
                }
            };

            Feedback send = new(this, context.Chat.Id);

            foreach (var handler in handlers)
            {
                if (await handler.HandleAsync(context, send))
                {
                    if (options.LogCommands && handler.Definition.LogHandling)
                    {
                        logger.LogInformation(
                            Events.MessageHandled,
                            LogMessages.MessageHandled_6,
                            context.Message.Text,
                            context.Message.Command,
                            context.Chat.Id.System,
                            context.Chat.Id.Type,
                            context.Chat.Id.Id,
                            context.Sender.Role
                        );
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                Events.MessageHandleException,
                ex,
                LogMessages.ErrorOccurred_0
            );
        }
        return;

        async Task<ChatContext> BuildChatContextAsync()
        {
            if (!_chatClients.TryGetValue(args.ChatId, out var properties))
            {
                properties = [];
                if (_chatClients.TryAdd(args.ChatId, properties))
                {
                    try
                    {
                        await chatStorage.AddChatAsync(
                            args.ChatId,
                            options.SaveChatTitles ? args.ChatTitle : string.Empty
                        );
                        logger.LogInformation(
                            Events.ChatAdded,
                            LogMessages.ChatAdded_1,
                            args.ChatId
                        );
                    }
                    catch
                    {
                        logger.LogWarning(
                            Events.ChatAddFailure,
                            LogMessages.ChatAddFailure_1,
                            args.ChatId
                        );
                        throw;
                    }
                }
                else
                {
                    properties = _chatClients[args.ChatId];
                }
            }

            return new ChatContext
            {
                Id = args.ChatId,
                Title = args.ChatTitle,
                Properties = properties
            };
        }

        (string command, string? tail) SplitMessage()
        {
            return args.Message.Split(" ", 2, StringSplitOptions.TrimEntries) is [{ } command, { } tail]
                ? (command, tail)
                : (args.Message, null);
        }

        ICommandHandler[] FindMatchedHandlers(string command, bool hasTail)
        {
            return
            [
                .. _commandHandlers
                .Where(
                    handler => handler.Definition.Matches(
                        command,
                        hasTail,
                        args.SenderRole
                    )
                )
                .OrderByDescending(
                    handler => handler.Definition.Priority
                )
            ];
        }

        MessageContext BuildMessageContext(string command, string? tail)
        {
            return new MessageContext
            {
                Id = args.MessageId,
                Text = args.Message,
                Command = command,
                Tail = tail ?? string.Empty,
                Args = tail is not null
                    ? MessageArgs.Parse(tail)
                    : MessageArgs.Empty
            };
        }
    }

    #region Static builder methods
    public static BotCoreBuilder CreateBuilder(
        IServiceProvider serviceProvider
    )
    {
        BotOptions options = ReadOptions(serviceProvider);

        return CreateBuilder(
            serviceProvider,
            options
        );
    }

    public static BotCoreBuilder CreateBuilder(
        IServiceProvider serviceProvider,
        Action<BotOptions> configureOptions
    )
    {
        BotOptions options = ReadOptions(serviceProvider);
        configureOptions(options);

        return CreateBuilder(
            serviceProvider,
            options
        );
    }

    public static BotCoreBuilder CreateBuilder(
        IServiceProvider serviceProvider,
        BotOptions options
    )
    {
        return new BotCoreBuilder(
            serviceProvider,
            options
        );
    }

    private static BotOptions ReadOptions(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        const string section = "Vermilion";

        return configuration.GetSection(section).Get<BotOptions>()
            ?? throw new InvalidOperationException(
                string.Format(
                    ExceptionMessages.CannotReadConfiguration_1,
                    section
                )
            );
    }
    #endregion
}