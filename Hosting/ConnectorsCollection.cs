using DioRed.Vermilion.Connectors;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Mutable builder used to configure connector instances.
/// </summary>
public class ConnectorsCollection(IServiceProvider services) : IConnectorsCollection
{
    internal Dictionary<string, IConnector> Connectors { get; } = [];

    /// <summary>
    /// Adds a connector under the specified key.
    /// </summary>
    public IConnectorsCollection Add(string key, IConnector connector)
    {
        Connectors.Add(key, connector);

        return this;
    }

    /// <summary>
    /// Adds multiple connectors.
    /// </summary>
    public IConnectorsCollection AddRange(params IEnumerable<KeyValuePair<string, IConnector>> connectors)
    {
        foreach (var connector in connectors)
        {
            Connectors.Add(connector.Key, connector.Value);
        }

        return this;
    }

    /// <summary>
    /// Adds a connector produced by the specified factory.
    /// </summary>
    public IConnectorsCollection Add(Func<IServiceProvider, KeyValuePair<string, IConnector>> factory)
    {
        (string key, IConnector connector) = factory.Invoke(services);

        return Add(key, connector);
    }

    /// <summary>
    /// Adds multiple connectors produced by the specified factory.
    /// </summary>
    public IConnectorsCollection Add(Func<IServiceProvider, IEnumerable<KeyValuePair<string, IConnector>>> factory)
    {
        IEnumerable<KeyValuePair<string, IConnector>> connectors = factory.Invoke(services);

        return AddRange(connectors);
    }
}
