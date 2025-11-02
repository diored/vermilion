using DioRed.Common.Jobs;
using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Jobs;
using DioRed.Vermilion.Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion.Hosting;

public class BotCoreBuilder
{
    private readonly Dictionary<string, IConnector> _connectors = [];
    private readonly List<ICommandHandler> _commandHandlers = [];
    private readonly List<IDailyJob> _dailyJobs = [];
    private readonly ILogger<BotCore> _botCoreLogger;

    private BotOptions? _options;
    private IChatStorage? _chatStorage;
    private ClientsPolicy _clientsPolicy = ClientsPolicy.All;

    internal BotCoreBuilder(
        IServiceProvider services
    )
    {
        Services = services;
        _botCoreLogger = Services.GetRequiredService<ILoggerFactory>().CreateLogger<BotCore>();
    }

    public IServiceProvider Services { get; }

    public BotCoreBuilder ConfigureCommandHandlers(
        IEnumerable<ICommandHandler> commandHandlers
    )
    {
        ArgumentNullException.ThrowIfNull(commandHandlers);

        if (_commandHandlers.Count > 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.CommandHandlersAlreadyInitialized_0
            );
        }

        ReadOnlySpan<ICommandHandler> commandHandlersSpan = [.. commandHandlers];

        if (commandHandlersSpan.Length == 0)
        {
            throw new ArgumentException(
                ExceptionMessages.CommandHandlersCannotBeEmpty_0,
                nameof(commandHandlers)
            );
        }

        _commandHandlers.Clear();
        _commandHandlers.AddRange(commandHandlersSpan);

        if (_commandHandlers.Count == 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.CommandHandlersCannotBeEmpty_0
            );
        }

        return this;
    }

    public BotCoreBuilder ConfigureCommandHandlers(
        Action<CommandHandlersCollection> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        CommandHandlersCollection collection = new(Services);
        configure(collection);

        return ConfigureCommandHandlers(collection.CommandHandlers);
    }

    public BotCoreBuilder ConfigureConnectors(Action<ConnectorsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (_connectors.Count > 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ConnectorsAlreadyInitialized_0
            );
        }

        ConnectorsCollection collection = new(Services);
        configure(collection);

        if (collection.Connectors.Count == 0)
        {
            throw new ArgumentException(
                ExceptionMessages.ConnectorsCannotBeEmpty_0,
                nameof(configure)
            );
        }

        foreach ((string key, IConnector connector) in collection.Connectors)
        {
            _connectors[key] = connector;
        }

        return this;
    }

    public BotCoreBuilder ConfigureDailyJobs(Action<DailyJobsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (_dailyJobs.Count > 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.DailyJobsAlreadyInitialized_0
            );
        }

        DailyJobsCollection collection = new(Services);
        configure(collection);

        _dailyJobs.Clear();
        _dailyJobs.AddRange(collection.DailyJobs);

        return this;
    }

    public BotCoreBuilder ConfigureChatStorage(Action<ChatStorageCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        ChatStorageCollection collection = new(Services);
        configure(collection);

        if (collection.ChatStorage is null)
        {
            throw new ArgumentException(
                ExceptionMessages.ChatStorageShouldBeInitialized_0,
                nameof(configure)
            );
        }

        _chatStorage = collection.ChatStorage;

        return this;
    }

    public BotCoreBuilder ConfigureClientsPolicy(Action<ClientPolicyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        ClientPolicyBuilder builder = new();
        configure(builder);

        _clientsPolicy = builder.Build();

        return this;
    }

    public BotCoreBuilder ConfigureOptions(BotOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;

        return this;
    }

    public BotCore Build()
    {
        BotCoreSettings settings = new()
        {
            ChatStorage = _chatStorage!,
            Connectors = _connectors,
            CommandHandlers = _commandHandlers,
            Options = _options ?? ReadBotOptions(),
            ClientsPolicy = _clientsPolicy,
        };

        BotCore botCore = new(settings, _botCoreLogger);

        foreach (IDailyJob dailyJob in _dailyJobs)
        {
            var job = Job.SetupDaily(
                () => dailyJob.Handle(Services, botCore),
                dailyJob.Definition.TimeOfDay,
                dailyJob.Definition.TimeZoneOffset,
                dailyJob.Definition.RepeatNumber,
                dailyJob.Definition.Id
            );

            job.Started += LogJobStarted;
            job.Finished += LogJobFinished;
            job.Scheduled += LogJobScheduled;

            job.Start();
        }

        return botCore;
    }

    private void LogJobStarted(object? sender, EventArgs eventArgs)
    {
        Job job = (Job)sender!;

        _botCoreLogger.LogInformation(
            Events.JobStarted,
            """Job "{JobId}" started""",
            job.Id
        );
    }

    private void LogJobFinished(object? sender, EventArgs eventArgs)
    {
        Job job = (Job)sender!;

        _botCoreLogger.LogInformation(
            Events.JobFinished,
            """Job "{JobId}" finished""",
            job.Id
        );
    }

    private void LogJobScheduled(object? sender, JobScheduledEventArgs eventArgs)
    {
        Job job = (Job)sender!;

        _botCoreLogger.LogInformation(
            Events.JobScheduled,
            """Next occurrence (#{OcurrenceNumber}) of the job "{JobId}" is scheduled at {NextOccurrence} (in {TimeLeft})""",
            eventArgs.OccurrenceNumber,
            job.Id,
            eventArgs.NextOccurrence.ToString("u"),
            (eventArgs.NextOccurrence - DateTimeOffset.Now).ToString("c")
        );
    }

    private BotOptions ReadBotOptions()
    {
        var configuration = Services.GetRequiredService<IConfiguration>();

        const string section = "Vermilion";

        return configuration.GetSection(section).Get<BotOptions>()
            ?? throw new InvalidOperationException(
                string.Format(
                    ExceptionMessages.CannotReadConfiguration_1,
                    section
                )
            );
    }
}