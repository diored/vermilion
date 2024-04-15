using System.Collections.Concurrent;

using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Interaction.Receivers;
using DioRed.Vermilion.Subsystems;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ChatProperties = System.Collections.Generic.Dictionary<string, object?>;

namespace DioRed.Vermilion;

public class BotCore : IHostedService
{
    public static readonly object _lock = new();

    private readonly IChatStorage _chatStorage;
    private readonly Dictionary<string, ISubsystem> _subsystems;
    private readonly List<ICommandHandler> _commandHandlers;
    private readonly ConcurrentDictionary<ChatId, ChatProperties> _chatClients;
    private readonly BotOptions _options;
    private readonly ILogger<BotCore> _logger;

    private bool _isStarted = false;

    internal BotCore(
        IChatStorage chatStorage,
        IEnumerable<KeyValuePair<string, ISubsystem>> subsystems,
        IEnumerable<ICommandHandler> commandHandlers,
        BotOptions options,
        ILogger<BotCore> logger
    )
    {
        _chatStorage = chatStorage;
        _subsystems = new Dictionary<string, ISubsystem>(subsystems);
        _commandHandlers = [.. commandHandlers];
        _options = options;
        _logger = logger;

        ChatId[] chats = chatStorage.GetChatsAsync().GetAwaiter().GetResult();

        _chatClients = new ConcurrentDictionary<ChatId, ChatProperties>(
            chats.ToDictionary(
                chatId => chatId,
                chatId => new ChatProperties()
            )
        );

        foreach (ISubsystem subsystem in _subsystems.Values)
        {
            subsystem.MessagePosted += (_, args) => OnMessagePosted(args);
        }

        if (_options.ShowCoreVersion)
        {
            _logger.LogInformation("DioRED Vermilion Core {Version} is started.", Version);
        }

        if (_options.Greeting is not null)
        {
            _logger.LogInformation("{Greeting}", _options.Greeting);
        }
    }

    public static string Version { get; } = typeof(BotCore).Assembly.GetName().Version?.ToString() ?? "0.0";

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isStarted)
            {
                throw new InvalidOperationException(
                    "Bot core is started already"
                );
            }

            _isStarted = true;
        }

        foreach (var subsystem in _subsystems)
        {
            await subsystem.Value.StartAsync(cancellationToken);

            _logger.LogInformation("{Subsystem} subsystem is started", subsystem.Key);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_isStarted)
            {
                throw new InvalidOperationException(
                    "Bot core is stopped already"
                );
            }

            _isStarted = false;
        }

        foreach (var subsystem in _subsystems)
        {
            await subsystem.Value.StopAsync(cancellationToken);

            _logger.LogInformation("{Subsystem} subsystem is stopped", subsystem.Key);
        }
    }

    public Task PostAsync(Receiver receiver, string text)
    {
        return PostAsync(receiver, new TextContent { Text = text });
    }

    public Task PostAsync(Receiver receiver, IContent content)
    {
        return PostAsync(receiver, _ => content);
    }

    public Task PostAsync(Receiver receiver, Func<ChatId, string> text)
    {
        return PostAsync(receiver, chatId => new TextContent { Text = text(chatId) });
    }

    public Task PostAsync(Receiver receiver, Func<ChatId, IContent> contentBuilder)
    {
        return PostAsync(receiver, chatId => Task.FromResult(contentBuilder(chatId)));
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
                _logger.LogInformation(
                    "Cannot post the content of type {ContentType} because it isn't supported by the target subsystem {Subsystem}",
                    content.GetType().Name,
                    chatId.System
                );
                break;

            case PostResult.BotBlocked:
                _logger.LogInformation(
                    "Bot has been blocked. Chat {ChatId} will not be counted as a client anymore",
                    chatId
                );
                await _chatStorage.RemoveChatAsync(chatId);
                _ = _chatClients.Remove(chatId, out _);
                break;

            case PostResult.UnhandledException:
                _logger.LogInformation(
                    "Unhandled exception occurred during posting the message to chat {ChatId}",
                    chatId
                );
                break;

            default:
                throw new InvalidOperationException(
                    $"Unexpected PostResult: {postResult}"
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
                    Role = args.SenderRole
                }
            };

            Feedback send = new(this, context.Chat.Id);

            foreach (var handler in handlers)
            {
                if (await handler.HandleAsync(context, send))
                {
                    if (_options.LogCommands && handler.Definition.LogHandling)
                    {
                        _logger.LogInformation(
                            1001,
                            """Message "{Message}" handled as a command "{Command}" in {System} {Type} chat #{ChatId} (user role: {UserRole})""",
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
            _logger.LogError(
                1900,
                ex,
                "Error occurred during message handling"
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
                    await _chatStorage.AddChatAsync(
                        args.ChatId,
                        _options.SaveChatTitles ? args.ChatTitle : string.Empty
                    );
                }
                else
                {
                    properties = _chatClients[args.ChatId];
                }
            }

            return new ChatContext
            {
                Id = args.ChatId,
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

        return configuration.GetSection("Vermilion").Get<BotOptions>()
            ?? throw new InvalidOperationException($"""Cannot read "Vermilion" value from the configuration""");
    }
    #endregion
}