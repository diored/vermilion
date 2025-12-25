using DioRed.Vermilion.Connectors;

namespace DioRed.Vermilion.Hosting;

public class ConnectorsCollection(IServiceProvider services) : IConnectorsCollection
{
    internal Dictionary<string, IConnector> Connectors { get; } = [];

    public IConnectorsCollection Add(string key, IConnector connector)
    {
        Connectors.Add(key, connector);

        return this;
    }

    public IConnectorsCollection AddRange(params IEnumerable<KeyValuePair<string, IConnector>> connectors)
    {
        foreach (var connector in connectors)
        {
            Connectors.Add(connector.Key, connector.Value);
        }

        return this;
    }

    public IConnectorsCollection Add(Func<IServiceProvider, KeyValuePair<string, IConnector>> factory)
    {
        (string key, IConnector connector) = factory.Invoke(services);

        return Add(key, connector);
    }

    public IConnectorsCollection Add(Func<IServiceProvider, IEnumerable<KeyValuePair<string, IConnector>>> factory)
    {
        IEnumerable<KeyValuePair<string, IConnector>> connectors = factory.Invoke(services);

        return AddRange(connectors);
    }
}