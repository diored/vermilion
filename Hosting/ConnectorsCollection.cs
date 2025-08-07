using DioRed.Vermilion.Connectors;

namespace DioRed.Vermilion.Hosting;
public class ConnectorsCollection(IServiceProvider services)
{
    internal Dictionary<string, IConnector> Connectors { get; } = [];

    public ConnectorsCollection Add(string key, IConnector connector)
    {
        Connectors.Add(key, connector);

        return this;
    }

    public ConnectorsCollection AddRange(params IEnumerable<KeyValuePair<string, IConnector>> connectors)
    {
        foreach (var connector in connectors)
        {
            Connectors.Add(connector.Key, connector.Value);
        }

        return this;
    }

    public ConnectorsCollection Add(Func<IServiceProvider, KeyValuePair<string, IConnector>> factory)
    {
        (string key, IConnector connector) = factory.Invoke(services);

        return Add(key, connector);
    }

    public ConnectorsCollection Add(Func<IServiceProvider, IEnumerable<KeyValuePair<string, IConnector>>> factory)
    {
        IEnumerable<KeyValuePair<string, IConnector>> connectors = factory.Invoke(services);

        return AddRange(connectors);
    }
}