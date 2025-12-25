using DioRed.Common.Jobs;
using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Handling;
using DioRed.Vermilion.Jobs;
using DioRed.Vermilion.Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DioRed.Vermilion.Hosting;

public class BotCoreBuilder
{
    private readonly Dictionary<string, IConnector> _connectors = [];
    private readonly List<ICommandHandler> _commandHandlers = [];
    private readonly List<IScheduledJob> _scheduledJobs = [];
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

    public BotCoreBuilder ConfigureScheduledJobs(Action<ScheduledJobsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (_scheduledJobs.Count > 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ScheduledJobsAlreadyInitialized_0
            );
        }

        ScheduledJobsCollection collection = new(Services);
        configure(collection);

        _scheduledJobs.Clear();
        _scheduledJobs.AddRange(collection.ScheduledJobs);

        return this;
    }

    public BotCoreBuilder ConfigureChatStorage(Action<ChatStorageCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (_chatStorage is not null)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ChatStorageAlreadyInitialized_0
            );
        }

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

    public BotCoreBuilder ConfigureOptions(Action<BotOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        BotOptions options = new();
        configure(options);

        _options = options;

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
        if (_chatStorage is null)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ChatStorageShouldBeInitialized_0
            );
        }

        if (_connectors.Count == 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ConnectorsCannotBeEmpty_0
            );
        }

        if (_commandHandlers.Count == 0)
        {
            throw new InvalidOperationException(
                ExceptionMessages.CommandHandlersCannotBeEmpty_0
            );
        }

        BotCoreSettings settings = new()
        {
            ChatStorage = _chatStorage!,
            Connectors = _connectors,
            CommandHandlers = _commandHandlers,
            Options = _options ?? ReadBotOptions(),
            ClientsPolicy = _clientsPolicy,
        };

        BotCore botCore = new(settings, _botCoreLogger);

        var appLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        foreach (var scheduled in _scheduledJobs)
        {
            ScheduledJobDefinition def = scheduled.Definition;

            JobOptions options = new()
            {
                MaxOccurrences = def.MaxOccurrences,
                MisfirePolicy = def.MisfirePolicy,
                MisfireThreshold = def.MisfireThreshold,
                MaxCatchUpExecutions = def.MaxCatchUpExecutions,
            };

            Job job = new(
                action: ct => scheduled.Handle(Services, botCore, ct),
                schedule: def.Schedule,
                options: options
            );

            string jobId = def.Id ?? job.Id;

            job.OccurrenceStarted += (_, e) => _botCoreLogger.LogInformation(
                    Events.JobStarted,
                    """Job "{JobId}" started (#{OccurrenceNumber})""",
                    jobId,
                    e.OccurrenceNumber
                );

            job.OccurrenceFinished += (_, e) => _botCoreLogger.LogInformation(
                Events.JobFinished,
                """Job "{JobId}" finished (#{OccurrenceNumber}) in {Duration}""",
                jobId,
                e.OccurrenceNumber,
                (e.FinishedAt - e.StartedAt).ToString("c")
            );

            job.Scheduled += (_, e) => _botCoreLogger.LogInformation(
                Events.JobScheduled,
                """Next occurrence (#{OccurrenceNumber}) of the job "{JobId}" is scheduled at {NextOccurrence} (in {TimeLeft})""",
                e.OccurrenceNumber,
                jobId,
                e.NextOccurrence.ToString("u"),
                (e.NextOccurrence - DateTimeOffset.Now).ToString("c")
            );

            job.Faulted += (_, e) => _botCoreLogger.LogError(
                Events.JobFailed,
                e.Exception,
                """Job "{JobId}" faulted (#{OccurrenceNumber})""",
                jobId,
                e.OccurrenceNumber
            );

            job.Cancelled += (_, _) => _botCoreLogger.LogInformation(
                Events.JobCancelled,
                """Job "{JobId}" cancelled""",
                jobId
            );

            job.Completed += (_, _) => _botCoreLogger.LogInformation(
                Events.JobCompleted,
                """Job "{JobId}" completed""",
                jobId
            );

            _ = RunJobSafely(job, appLifetime.ApplicationStopping, jobId);
        }

        return botCore;
    }

    private async Task RunJobSafely(Job job, CancellationToken stopping, string jobId)
    {
        try
        {
            await job.StartAsync(stopping).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // ok
        }
        catch (Exception ex)
        {
            _botCoreLogger.LogError(ex, """Job "{JobId}" crashed in wrapper""", jobId);
        }
    }

    private BotOptions ReadBotOptions()
    {
        IConfiguration configuration = Services.GetRequiredService<IConfiguration>();
        IConfigurationSection section = configuration.GetRequiredSection("Vermilion");

        return new BotOptions
        {
            // Section is already "Vermilion", so read keys relative to it.
            Greeting = section.GetValue<string?>("Greeting"),
            LogCommands = section.GetValue<bool?>("LogCommands") ?? true,
            SaveChatTitles = section.GetValue<bool?>("SaveChatTitles") ?? true,
            ShowCoreVersion = section.GetValue<bool?>("ShowCoreVersion") ?? true
        };
    }
}