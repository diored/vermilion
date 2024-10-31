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

namespace DioRed.Vermilion;

public class BotCore(
    IChatStorage chatStorage,
    IEnumerable<KeyValuePair<string, ISubsystem>> subsystems,
    IEnumerable<ICommandHandler> commandHandlers,
    BotOptions options,
    ILogger<BotCore> logger,
    Func<ChatId, bool> chatClientEligibility
) : IHostedService
{
    public static readonly object _lock = new();

    private readonly Dictionary<string, ISubsystem> _subsystems = new(subsystems);
    private readonly ICommandHandler[] _commandHandlers = [.. commandHandlers];
    private readonly ConcurrentDictionary<ChatId, ChatClient> _chatClients = [];

    public BotCoreState State { get; private set; } = BotCoreState.NotInitialized;

    public static string Version { get; } = typeof(BotCore).Assembly.GetName().Version.Normalize();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (State is BotCoreState.NotInitialized)
            {
                Initialize();
            }

            if (State is not (BotCoreState.Initialized or BotCoreState.Stopped))
            {
                throw new InvalidOperationException(
                    string.Format(
                        ExceptionMessages.CannotStartBotCoreInState_1,
                        State
                    )
                );
            }

            State = BotCoreState.Starting;
        }

        bool atLeastOneSubsystemStarted = false;

        foreach (var subsystem in _subsystems)
        {
            try
            {
                await subsystem.Value.StartAsync(cancellationToken);

                atLeastOneSubsystemStarted = true;

                if (subsystem.Value.Version is { } version)
                {
                    logger.LogInformation(
                        LogMessages.SubsystemStarted_2,
                        subsystem.Key,
                        version
                    );
                }
                else
                {
                    logger.LogInformation(
                        LogMessages.SubsystemStarted_1,
                        subsystem.Key
                    );
                }
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

        State = atLeastOneSubsystemStarted
            ? BotCoreState.Started
            : BotCoreState.Stopped;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (State is BotCoreState.Stopped)
            {
                logger.LogInformation(
                    LogMessages.BotCoreAlreadyStopped_0
                );
                return;
            }

            if (State is not BotCoreState.Started)
            {
                throw new InvalidOperationException(
                    string.Format(
                        ExceptionMessages.CannotStopBotCoreInState_1,
                        State
                    )
                );
            }

            State = BotCoreState.Stopping;
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

        State = atLeastOneSubsystemFailedToStop
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

    public Task PostAsync(Receiver receiver, Func<ChatInfo, string> text)
    {
        return PostAsync(
            receiver,
            chatId => new TextContent
            {
                Text = text(chatId)
            }
        );
    }

    public Task PostAsync(Receiver receiver, Func<ChatInfo, IContent> contentBuilder)
    {
        return PostAsync(
            receiver,
            chatId => Task.FromResult(
                contentBuilder(chatId)
            )
        );
    }

    public async Task PostAsync(Receiver receiver, Func<ChatInfo, Task<IContent>> contentBuilder)
    {
        ChatInfo[] chats = GetChats(receiver);

        foreach (ChatInfo chat in chats)
        {
            IContent content = await contentBuilder(chat);

            PostResult postResult = await _subsystems[chat.ChatId.System].PostAsync(
                chat.ChatId.Id,
                content
            );

            await ProcessResultAsync(postResult, chat, content);
        }
    }

    internal async Task AddTagAsync(Receiver receiver, string tag)
    {
        ChatInfo[] chats = GetChats(receiver);

        foreach (ChatInfo chat in chats)
        {
            if (chat.Tags.Contains(tag))
            {
                continue;
            }

            ChatInfo updatedChat = new ChatInfo
            {
                ChatId = chat.ChatId,
                Tags = [.. chat.Tags, tag]
            };

            await chatStorage.UpdateChatAsync(updatedChat);
            await ReloadChatClientAsync(chat.ChatId);
        }
    }

    internal async Task RemoveTagAsync(Receiver receiver, string tag)
    {
        ChatInfo[] chats = GetChats(receiver);

        foreach (ChatInfo chat in chats)
        {
            if (!chat.Tags.Contains(tag))
            {
                continue;
            }

            ChatInfo updatedChat = new ChatInfo
            {
                ChatId = chat.ChatId,
                Tags = [.. chat.Tags.Except([tag])]
            };

            await chatStorage.UpdateChatAsync(updatedChat);
            await ReloadChatClientAsync(chat.ChatId);
        }
    }

    private void Initialize()
    {
        lock (_lock)
        {
            if (State != BotCoreState.NotInitialized)
            {
                throw new InvalidOperationException(
                    ExceptionMessages.BotCoreAlreadyInitialized_0
                );
            }

            State = BotCoreState.Initializing;
        }

        try
        {
            ChatInfo[] chats = chatStorage.GetChatsAsync().GetAwaiter().GetResult();

            foreach (ChatInfo chatInfo in chats)
            {
                _ = _chatClients.TryAdd(
                    chatInfo.ChatId,
                    new ChatClient
                    {
                        ChatInfo = chatInfo,
                        Properties = []
                    }
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

            State = BotCoreState.Initialized;
        }
        catch
        {
            State = BotCoreState.NotInitialized;
            throw;
        }
    }

    private ChatInfo[] GetChats(Receiver receiver)
    {
        Func<ChatInfo, bool> selector = receiver switch
        {
            SingleChatReceiver single => (ChatInfo chatInfo) => chatInfo.ChatId == single.ChatId,
            BroadcastReceiver broadcast => (ChatInfo chatInfo) => broadcast.Filter(chatInfo),
            EveryoneReceiver => (ChatInfo chatInfo) => true,
            _ => throw new ArgumentOutOfRangeException(nameof(receiver), receiver, null)
        };

        return [.. _chatClients.Select(x => x.Value.ChatInfo).Where(selector)];
    }

    private async Task ProcessResultAsync(PostResult postResult, ChatInfo chat, IContent content)
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
                    chat.ChatId.System
                );
                break;

            case PostResult.ChatAccessDenied:
                logger.LogInformation(
                    LogMessages.AccessDenied_1,
                    chat
                );

                try
                {
                    await chatStorage.RemoveChatAsync(chat.ChatId);
                    _ = _chatClients.Remove(chat.ChatId, out _);
                    logger.LogInformation(
                        Events.ChatRemoved,
                        LogMessages.ChatRemoved_1,
                        chat
                    );
                }
                catch
                {
                    logger.LogWarning(
                        Events.ChatRemoveFailure,
                        LogMessages.ChatRemoveFailure_1,
                        chat
                    );
                    throw;
                }
                break;

            case PostResult.SubsystemFailure:
                logger.LogInformation(
                    LogMessages.MessageDeliveryFailed_1,
                    chat
                );
                break;

            case PostResult.UnexpectedException:
                logger.LogInformation(
                    LogMessages.UnexpectedException_1,
                    chat
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
            (string command, string? tail) = SplitMessage();

            ICommandHandler[] handlers = FindMatchedHandlers(
                command,
                hasTail: tail is not null,
                clientIsEligible: chatClientEligibility(args.ChatId)
            );

            if (handlers.Length == 0)
            {
                return;
            }

            ChatContext chatContext = await BuildChatContextAsync();

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

            Feedback feedback = new(this, context.Chat.Id);

            foreach (var handler in handlers)
            {
                if (await handler.HandleAsync(context, feedback))
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
            if (!_chatClients.TryGetValue(args.ChatId, out var chatClient))
            {
                ChatInfo chatInfo = new() { ChatId = args.ChatId };

                chatClient = new ChatClient
                {
                    ChatInfo = chatInfo
                };

                if (_chatClients.TryAdd(args.ChatId, chatClient))
                {
                    try
                    {
                        await chatStorage.AddChatAsync(
                            chatInfo,
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
                    chatClient = _chatClients[args.ChatId];
                }
            }

            return new ChatContext
            {
                Id = args.ChatId,
                Title = args.ChatTitle,
                Tags = chatClient.ChatInfo.Tags,
                Properties = chatClient.Properties
            };
        }

        (string command, string? tail) SplitMessage()
        {
            return args.Message.Split(" ", 2, StringSplitOptions.TrimEntries) is [{ } command, { } tail]
                ? (command, tail)
                : (args.Message.Trim(), null);
        }

        ICommandHandler[] FindMatchedHandlers(string command, bool hasTail, bool clientIsEligible)
        {
            return
            [
                .. _commandHandlers
                .Where(
                    handler => handler.Definition.Matches(
                        command,
                        hasTail,
                        args.SenderRole,
                        clientIsEligible
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

    private async Task ReloadChatClientAsync(ChatId chatId)
    {
        var properties = _chatClients.TryGetValue(chatId, out var current)
            ? current.Properties
            : [];

        ChatInfo chatInfo = await chatStorage.GetChatAsync(chatId);
        _chatClients[chatId] = new ChatClient
        {
            ChatInfo = chatInfo,
            Properties = properties
        };
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