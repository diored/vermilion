using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Extensions;
using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;
using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Interaction.Receivers;
using DioRed.Vermilion.Messages;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

public class BotCore : IHostedService
{
    private readonly Lock _sync = new();

    private readonly IChatStorage _chatStorage;
    private readonly ConnectorsManager _connectors;
    private readonly CommandHandlersManager _commandHandlers;
    private readonly ChatClientsManager _chatClientsManager;
    private readonly BotOptions _options;
    private readonly ClientsPolicy _clientsPolicy;
    private readonly ILogger<BotCore> _logger;

    public BotCore(
        BotCoreSettings settings,
        ILogger<BotCore> logger
    )
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        Validate(settings);

        _chatStorage = settings.ChatStorage;
        _connectors = new ConnectorsManager(settings.Connectors);
        _commandHandlers = new CommandHandlersManager(settings.CommandHandlers);
        _chatClientsManager = settings.ChatClientsManager;
        _options = settings.Options;
        _clientsPolicy = settings.ClientsPolicy;

        _logger = logger;
    }

    private static void Validate(BotCoreSettings settings)
    {
        if (settings.ChatStorage is null)
        {
            throw new ArgumentException(
                ExceptionMessages.ChatStorageShouldBeInitialized_0,
                nameof(settings)
            );
        }

        if (settings.Connectors is null or { Count: 0 })
        {
            throw new ArgumentException(
                ExceptionMessages.ConnectorsShouldBeInitialized_0,
                nameof(settings)
            );
        }

        if (settings.CommandHandlers is null or { Count: 0 })
        {
            throw new ArgumentException(
                ExceptionMessages.CommandHandlersShouldBeInitialized_0,
                nameof(settings)
            );
        }

        if (settings.ChatClientsManager is null)
        {
            throw new ArgumentException(
                ExceptionMessages.ChatClientsManagerShouldBeInitialized_0,
                nameof(settings)
            );
        }

        if (settings.Options is null)
        {
            throw new ArgumentException(
                ExceptionMessages.BotOptionsShouldBeInitialized_0,
                nameof(settings)
            );
        }

        if (settings.ClientsPolicy is null)
        {
            throw new ArgumentException(
                ExceptionMessages.ClientsPolicyShouldBeInitialized_0,
                nameof(settings)
            );
        }
    }

    public static string Version { get; } = typeof(BotCore).Assembly.GetName().Version.Normalize();

    public BotCoreState State { get; private set; } = BotCoreState.NotInitialized;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        bool needsInitialization = false;

        lock (_sync)
        {
            if (State is BotCoreState.NotInitialized)
            {
                State = BotCoreState.Initializing;
                needsInitialization = true;
            }

            if (State is not (BotCoreState.Initializing or BotCoreState.Initialized or BotCoreState.Stopped))
            {
                throw new InvalidOperationException(
                    string.Format(
                        ExceptionMessages.CannotStartBotCoreInState_1,
                        State
                    )
                );
            }
        }

        if (needsInitialization)
        {
            try
            {
                await InitializeAsync().ConfigureAwait(false);
            }
            catch
            {
                lock (_sync)
                {
                    State = BotCoreState.NotInitialized;
                }
                throw;
            }
        }

        lock (_sync)
        {
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

        bool atLeastOneConnectorStarted = false;

        foreach ((string key, IConnector connector) in _connectors.Enumerate())
        {
            try
            {
                await connector.StartAsync(cancellationToken);

                atLeastOneConnectorStarted = true;

                if (connector.Version is { } version)
                {
                    _logger.LogInformation(
                        LogMessages.ConnectorStarted_2,
                        key,
                        version
                    );
                }
                else
                {
                    _logger.LogInformation(
                        LogMessages.ConnectorStarted_1,
                        key
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    LogMessages.ConnectorNotStarted_1,
                    key
                );
            }
        }

        State = atLeastOneConnectorStarted
            ? BotCoreState.Started
            : BotCoreState.Stopped;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (State is BotCoreState.Stopped)
            {
                _logger.LogInformation(
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

        bool atLeastOneConnectorFailedToStop = false;

        foreach ((string key, IConnector connector) in _connectors.Enumerate())
        {
            try
            {
                await connector.StopAsync(cancellationToken);

                _logger.LogInformation(
                    LogMessages.ConnectorStopped_1,
                    key
                );
            }
            catch (Exception ex)
            {
                atLeastOneConnectorFailedToStop = true;

                _logger.LogError(
                    ex,
                    LogMessages.ConnectorNotStopped_1,
                    key
                );
            }
        }

        State = atLeastOneConnectorFailedToStop
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

    public Task PostAsync(Receiver receiver, Func<ChatMetadata, string> textBuilder)
    {
        return PostAsync(
            receiver,
            chatMetadata => new TextContent
            {
                Text = textBuilder(chatMetadata)
            }
        );
    }

    public Task PostAsync(Receiver receiver, Func<ChatMetadata, IContent> contentBuilder)
    {
        return PostAsync(
            receiver,
            chatMetadata => Task.FromResult(
                contentBuilder(chatMetadata)
            )
        );
    }

    public async Task PostAsync(Receiver receiver, Func<ChatMetadata, Task<IContent>> contentBuilder)
    {
        ChatClient[] chatClients = GetChatClientsByReceiver(receiver);

        foreach (ChatClient chatClient in chatClients)
        {
            IContent content = await contentBuilder(chatClient.Metadata);
            IConnector connector = _connectors[chatClient.Metadata.ChatId.ConnectorKey];

            PostResult postResult = await connector.PostAsync(
                chatClient.Metadata.ChatId.Id,
                content
            );

            await ProcessResultAsync(postResult, chatClient.Metadata, content);
        }
    }

    internal async Task AddTagAsync(Receiver receiver, string tag)
    {
        ChatClient[] chatClients = GetChatClientsByReceiver(receiver);

        foreach (ChatClient chatClient in chatClients)
        {
            if (chatClient.Metadata.Tags.Contains(tag))
            {
                continue;
            }

            chatClient.Metadata.Tags.Add(tag);

            await _chatStorage.UpdateChatAsync(chatClient.Metadata);
            await ReloadChatClientAsync(chatClient.Metadata.ChatId);
        }
    }

    internal async Task RemoveTagAsync(Receiver receiver, string tag)
    {
        ChatClient[] chatClients = GetChatClientsByReceiver(receiver);

        foreach (ChatClient chatClient in chatClients)
        {
            if (!chatClient.Metadata.Tags.Contains(tag))
            {
                continue;
            }

            chatClient.Metadata.Tags.Remove(tag);

            await _chatStorage.UpdateChatAsync(chatClient.Metadata);
            await ReloadChatClientAsync(chatClient.Metadata.ChatId);
        }
    }

    private async Task InitializeAsync()
    {
        // Load saved chats.
        ChatMetadata[] chats = await _chatStorage.GetChatsAsync().ConfigureAwait(false);

        foreach (ChatMetadata chatMetadata in chats)
        {
            _chatClientsManager.Add(chatMetadata);
        }

        // Subscribe to connectors.
        foreach ((_, IConnector connector) in _connectors.Enumerate())
        {
            connector.MessagePosted += (_, args) => OnMessagePosted(args);
        }

        if (_options.ShowCoreVersion)
        {
            _logger.LogInformation(
                LogMessages.CoreVersionInfo_1,
                Version
            );
        }

        if (_options.Greeting is not null)
        {
            _logger.LogInformation(
                LogMessages.CustomGreeting_1,
                _options.Greeting
            );
        }

        lock (_sync)
        {
            State = BotCoreState.Initialized;
        }
    }

    private ChatClient[] GetChatClientsByReceiver(Receiver receiver)
    {
        return receiver switch
        {
            SingleChatReceiver single => _chatClientsManager.Get(single.ChatId) is { } chatClient
                ? [chatClient]
                : [],
            BroadcastReceiver broadcast => _chatClientsManager.Find(broadcast.Filter),
            EveryoneReceiver => _chatClientsManager.GetAll(),
            _ => throw new ArgumentOutOfRangeException(nameof(receiver), receiver, null)
        };
    }

    private async Task ProcessResultAsync(
        PostResult postResult,
        ChatMetadata chatMetadata,
        IContent content
    )
    {
        switch (postResult)
        {
            case PostResult.Success:
                // do nothing
                break;

            case PostResult.ContentTypeNotSupported:
                _logger.LogInformation(
                    LogMessages.UnsupportedContent_2,
                    content.GetType().Name,
                    chatMetadata.ChatId.ConnectorKey
                );
                break;

            case PostResult.ChatAccessDenied:
                _logger.LogInformation(
                    LogMessages.AccessDenied_1,
                    chatMetadata
                );

                try
                {
                    await _chatStorage.RemoveChatAsync(chatMetadata.ChatId);
                    _chatClientsManager.Remove(chatMetadata.ChatId);

                    _logger.LogInformation(
                        Events.ChatRemoved,
                        LogMessages.ChatRemoved_1,
                        chatMetadata
                    );
                }
                catch
                {
                    _logger.LogWarning(
                        Events.ChatRemoveFailure,
                        LogMessages.ChatRemoveFailure_1,
                        chatMetadata
                    );
                    throw;
                }
                break;

            case PostResult.ConnectorFailure:
                _logger.LogInformation(
                    LogMessages.MessageDeliveryFailed_1,
                    chatMetadata
                );
                break;

            case PostResult.UnexpectedException:
                _logger.LogInformation(
                    LogMessages.UnexpectedException_1,
                    chatMetadata
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

            ICommandHandler[] handlers = _commandHandlers.FindMatchedHandlers(
                command,
                hasTail: tail is not null,
                senderRole: args.SenderRole,
                clientIsEligible: _clientsPolicy.IsEligible(args.ChatId)
            );

            if (handlers.Length == 0)
            {
                return;
            }

            ChatContext chatContext = await BuildChatContextAsync();
            MessageContext messageContext = BuildMessageContext(command, tail);
            SenderContext senderContext = new()
            {
                Id = args.SenderId,
                Role = args.SenderRole,
                Name = args.SenderName
            };

            MessageHandlingContext context = new()
            {
                Chat = chatContext,
                Message = messageContext,
                Sender = senderContext
            };

            Feedback feedback = new(this, context.Chat.Id);

            foreach (var handler in handlers)
            {
                if (await handler.HandleAsync(context, feedback))
                {
                    if (_options.LogCommands && handler.Definition.LogHandling)
                    {
                        _logger.LogInformation(
                            Events.MessageHandled,
                            LogMessages.MessageHandled_6,
                            context.Message.Text,
                            context.Message.Command,
                            context.Chat.Id.ConnectorKey,
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
            _logger.LogError(
                Events.MessageHandleException,
                ex,
                LogMessages.ErrorOccurred_0
            );
        }
        return;

        async Task<ChatContext> BuildChatContextAsync()
        {
            if (_chatClientsManager.Get(args.ChatId) is not { } chatClient)
            {
                ChatMetadata chatMetadata = new() { ChatId = args.ChatId };

                if (_chatClientsManager.Add(chatMetadata))
                {
                    try
                    {
                        if (_options.SaveChatTitles)
                        {
                            await _chatStorage.AddChatAsync(chatMetadata, args.ChatTitle);
                        }
                        else
                        {
                            await _chatStorage.AddChatAsync(chatMetadata);
                        }

                        _logger.LogInformation(
                            Events.ChatAdded,
                            LogMessages.ChatAdded_1,
                            args.ChatId
                        );
                    }
                    catch
                    {
                        _logger.LogWarning(
                            Events.ChatAddFailure,
                            LogMessages.ChatAddFailure_1,
                            args.ChatId
                        );
                        throw;
                    }
                }

                chatClient = _chatClientsManager.Get(args.ChatId)!;
            }

            return new ChatContext
            {
                Client = chatClient,
                Title = args.ChatTitle,
                Connector = _connectors[chatClient.Metadata.ChatId.ConnectorKey]
            };
        }

        (string command, string? tail) SplitMessage()
        {
            return args.Message.Split(" ", 2, StringSplitOptions.TrimEntries) is [{ } command, { } tail]
                ? (command, tail)
                : (args.Message.Trim(), null);
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
        var runtimeValues = _chatClientsManager.Get(chatId)?.RuntimeValues ?? [];

        ChatMetadata metadata = await _chatStorage.GetChatAsync(chatId);

        _chatClientsManager.Set(chatId, new ChatClient
        {
            Metadata = metadata,
            RuntimeValues = runtimeValues
        });
    }
}