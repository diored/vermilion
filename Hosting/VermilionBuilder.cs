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

    public IServiceCollection Services { get; }

    internal VermilionBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public VermilionBuilder ConfigureChatStorage(Action<ChatStorageCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureChatStorage(configure));
        return this;
    }

    public VermilionBuilder ConfigureConnectors(Action<ConnectorsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureConnectors(configure));
        return this;
    }

    public VermilionBuilder ConfigureCommandHandlers(Action<CommandHandlersCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureCommandHandlers(configure));
        return this;
    }

    public VermilionBuilder ConfigureScheduledJobs(Action<ScheduledJobsCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureScheduledJobs(configure));
        return this;
    }

    public VermilionBuilder ConfigureClientsPolicy(Action<ClientPolicyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureClientsPolicy(configure));
        return this;
    }

    public VermilionBuilder ConfigureOptions(Action<BotOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        BotCoreConfigurators.Add(b => b.ConfigureOptions(configure));
        return this;
    }

    public VermilionBuilder ConfigureOptions(BotOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        BotCoreConfigurators.Add(b => b.ConfigureOptions(options));
        return this;
    }
}
