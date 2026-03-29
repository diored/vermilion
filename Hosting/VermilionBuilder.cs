using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// A configuration surface for Vermilion that executes during <c>ConfigureServices</c>,
/// allowing plugins to register their own services in DI and to defer Vermilion-specific
/// configuration until the service provider is built.
/// </summary>
public sealed class VermilionBuilder
{
    internal List<Action<BotCoreBuilder>> BotCoreConfigurators { get; } = [];

    /// <summary>
    /// Gets the service collection used by the host.
    /// </summary>
    public IServiceCollection Services { get; }

    internal VermilionBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Configures the chat storage used by Vermilion.
    /// </summary>
    public VermilionBuilder ConfigureChatStorage(Action<ChatStorageCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureChatStorage(configure));
        return this;
    }

    /// <summary>
    /// Uses the specified chat storage configuration.
    /// </summary>
    public VermilionBuilder UseChatStorage(Action<ChatStorageCollection> configure)
        => ConfigureChatStorage(configure);

    /// <summary>
    /// Configures the connectors used by Vermilion.
    /// </summary>
    public VermilionBuilder ConfigureConnectors(Action<ConnectorsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureConnectors(configure));
        return this;
    }

    /// <summary>
    /// Configures command handlers used by Vermilion.
    /// </summary>
    public VermilionBuilder ConfigureCommandHandlers(Action<CommandHandlersCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureCommandHandlers(configure));
        return this;
    }

    /// <summary>
    /// Configures scheduled jobs used by Vermilion.
    /// </summary>
    public VermilionBuilder ConfigureScheduledJobs(Action<ScheduledJobsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureScheduledJobs(configure));
        return this;
    }

    /// <summary>
    /// Legacy alias preserved for migration from older Vermilion versions.
    /// </summary>
    [Obsolete("Use ConfigureScheduledJobs instead.")]
    public VermilionBuilder ConfigureDailyJobs(Action<ScheduledJobsCollection> configure)
        => ConfigureScheduledJobs(configure);

    /// <summary>
    /// Configures the clients policy used by Vermilion.
    /// </summary>
    public VermilionBuilder ConfigureClientsPolicy(Action<ClientPolicyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureClientsPolicy(configure));
        return this;
    }

    /// <summary>
    /// Configures bot options through a delegate.
    /// </summary>
    public VermilionBuilder ConfigureOptions(Action<BotOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureOptions(configure));
        return this;
    }

    /// <summary>
    /// Uses the specified bot options instance.
    /// </summary>
    public VermilionBuilder ConfigureOptions(BotOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        BotCoreConfigurators.Add(b => b.ConfigureOptions(options));
        return this;
    }

    /// <summary>
    /// Loads command handlers from the entry assembly.
    /// </summary>
    public VermilionBuilder LoadCommandHandlersFromEntryAssembly()
        => ConfigureCommandHandlers(c => c.LoadFromEntryAssembly());

    /// <summary>
    /// Loads command handlers from the specified assembly.
    /// </summary>
    public VermilionBuilder LoadCommandHandlersFromAssembly(Assembly assembly)
        => ConfigureCommandHandlers(c => c.LoadFromAssembly(assembly));

    /// <summary>
    /// Loads scheduled jobs from the entry assembly.
    /// </summary>
    public VermilionBuilder LoadScheduledJobsFromEntryAssembly()
        => ConfigureScheduledJobs(c => c.LoadFromEntryAssembly());

    /// <summary>
    /// Loads scheduled jobs from the specified assembly.
    /// </summary>
    public VermilionBuilder LoadScheduledJobsFromAssembly(Assembly assembly)
        => ConfigureScheduledJobs(c => c.LoadFromAssembly(assembly));

    /// <summary>
    /// Allows outgoing messages only for the specified chats.
    /// </summary>
    public VermilionBuilder AllowFor(params IEnumerable<ChatId> chatIds)
        => ConfigureClientsPolicy(c => c.AllowFor(chatIds));

    /// <summary>
    /// Allows outgoing messages only for chats that satisfy the specified condition.
    /// </summary>
    public VermilionBuilder AllowFor(Func<ChatId, bool> condition)
        => ConfigureClientsPolicy(c => c.AllowFor(condition));

    /// <summary>
    /// Allows outgoing messages for every chat.
    /// </summary>
    public VermilionBuilder AllowForEveryone()
        => ConfigureClientsPolicy(c => c.AllowForEveryone());
}
