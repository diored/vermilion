using System.Reflection;

using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Jobs;
using DioRed.Vermilion.L10n;
using DioRed.Vermilion.Subsystems;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion;

public class BotCoreBuilder
{
    private readonly BotOptions _options;
    private readonly Dictionary<string, ISubsystem> _subsystems = [];
    private readonly List<ICommandHandler> _commandHandlers = [];
    private readonly List<IDailyJob> _dailyJobs = [];
    private readonly ILogger<BotCore> _botCoreLogger;

    private IChatStorage? _chatStorage;
    private Func<ChatId, bool> _chatClientEligibility;

    internal BotCoreBuilder(
        IServiceProvider serviceProvider,
        BotOptions options
    )
    {
        Services = serviceProvider;
        _options = options;
        _botCoreLogger = Services.GetRequiredService<ILoggerFactory>().CreateLogger<BotCore>();
        _chatClientEligibility = _ => true;
    }

    public IServiceProvider Services { get; }

    public BotCoreBuilder AddCommand(
        string command,
        Func<string, string> replyFunc,
        UserRole requiredRole = UserRole.Member
    )
    {
        return AddCommandHandler(
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = command,
                    HasTail = true,
                    RequiredRole = requiredRole
                },
                (context, send) => send.TextAsync(replyFunc(context.Message.Tail))
            )
        );
    }

    public BotCoreBuilder AddCommand(
        string command,
        Func<string> replyFunc,
        UserRole requiredRole = UserRole.Member
    )
    {
        return AddCommandHandler(
            new SimpleCommandHandler(
                new CommandDefinition
                {
                    Template = command,
                    HasTail = false,
                    RequiredRole = requiredRole
                },
                (context, send) => send.TextAsync(replyFunc())
            )
        );
    }

    public BotCoreBuilder AddCommandHandler(ICommandHandler commandHandler)
    {
        _commandHandlers.Add(commandHandler);

        return this;
    }

    public BotCoreBuilder AddCommandHandler(Type type)
    {
        if (!typeof(ICommandHandler).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                string.Format(
                    ExceptionMessages.TypeDoesntImplementTheInterface_2,
                    type.Name,
                    nameof(ICommandHandler)
                ),
                nameof(type)
            );
        }

        return AddCommandHandler((ICommandHandler)Resolve(type));
    }

    public BotCoreBuilder AddCommandHandler<T>()
        where T : ICommandHandler
    {
        return AddCommandHandler(Resolve<T>());
    }

    public BotCoreBuilder AddCommandHandlersFromAssembly(Assembly assembly)
    {
        TypeInfo[] commandHandlers =
        [
            .. assembly.DefinedTypes
                .Where(typeInfo =>
                    typeInfo.IsClass &&
                    !typeInfo.IsAbstract &&
                    typeInfo.ImplementedInterfaces.Contains(typeof(ICommandHandler)))
        ];

        foreach (TypeInfo typeInfo in commandHandlers)
        {
            AddCommandHandler(typeInfo.AsType());
        }

        return this;
    }

    public BotCoreBuilder AddDailyJob(IDailyJob dailyJob)
    {
        _dailyJobs.Add(dailyJob);

        return this;
    }

    public BotCoreBuilder AddDailyJob(Type type)
    {
        if (!typeof(IDailyJob).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                string.Format(
                    ExceptionMessages.TypeDoesntImplementTheInterface_2,
                    type.Name,
                    nameof(IDailyJob)
                ),
                nameof(type)
            );
        }

        return AddDailyJob((IDailyJob)Resolve(type));
    }

    public BotCoreBuilder AddDailyJob<T>()
        where T : IDailyJob
    {
        return AddDailyJob(Resolve<T>());
    }

    public BotCoreBuilder AddDailyJobsFromAssembly(Assembly assembly)
    {
        TypeInfo[] dailyJobs =
        [
            .. assembly.DefinedTypes
                .Where(typeInfo =>
                    typeInfo.IsClass &&
                    !typeInfo.IsAbstract &&
                    typeInfo.ImplementedInterfaces.Contains(typeof(IDailyJob)))
        ];

        foreach (TypeInfo typeInfo in dailyJobs)
        {
            AddDailyJob(typeInfo.AsType());
        }

        return this;
    }

    public BotCoreBuilder AddSubsystem(
        string system,
        ISubsystem subsystem
    )
    {
        _subsystems.Add(system, subsystem);

        return this;
    }

    public BotCoreBuilder AllowForChats(Func<ChatId, bool> chatClientEligibility)
    {
        _chatClientEligibility = chatClientEligibility;

        return this;
    }

    public BotCoreBuilder AllowForChats(IEnumerable<ChatId> chatIds)
    {
        ChatId[] chats = [.. chatIds];

        return AllowForChats(chats.Contains);
    }

    public BotCore Build()
    {
        foreach (IDailyJob dailyJob in _dailyJobs)
        {
            Services.SetupDailyJob(
                dailyJob.Handle,
                dailyJob.Definition.TimeOfDay,
                dailyJob.Definition.TimeZoneOffset,
                dailyJob.Definition.RepeatNumber,
                dailyJob.Definition.Id
            );

        }
        return new BotCore(
            _chatStorage ?? Services.GetRequiredService<IChatStorage>(),
            _subsystems,
            _commandHandlers,
            _options.Clone(),
            _botCoreLogger,
            _chatClientEligibility
        );
    }

    public BotCoreBuilder UseChatStorage(IChatStorage chatStorage)
    {
        if (_chatStorage is not null)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ChatStorageAlreadyInitialized_0
            );
        }

        _chatStorage = chatStorage;

        return this;
    }

    public BotCoreBuilder UseChatStorage<T>()
        where T : IChatStorage
    {
        return UseChatStorage(Resolve<T>());
    }

    private object Resolve(Type type)
    {
        return Services.GetService(type) ?? ActivatorUtilities.CreateInstance(
            Services,
            type
        );
    }

    private T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }
}