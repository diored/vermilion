namespace DioRed.Vermilion.Connectors;

public class ConnectorsManager(IEnumerable<KeyValuePair<string, IConnector>> connectors)
{
    private readonly Dictionary<string, IConnector> _connectors = new(connectors);

    public IConnector this[string key]
    {
        get => _connectors[key];
        set => _connectors[key] = value;
    }

    public IEnumerable<KeyValuePair<string, IConnector>> Enumerate()
    {
        return _connectors.AsEnumerable();
    }
}