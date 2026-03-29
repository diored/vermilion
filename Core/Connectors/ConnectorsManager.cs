namespace DioRed.Vermilion.Connectors;

/// <summary>
/// Provides keyed access to configured connectors.
/// </summary>
public class ConnectorsManager(IEnumerable<KeyValuePair<string, IConnector>> connectors)
{
    private readonly Dictionary<string, IConnector> _connectors = new(connectors);

    /// <summary>
    /// Gets or sets a connector by key.
    /// </summary>
    public IConnector this[string key]
    {
        get => _connectors[key];
        set => _connectors[key] = value;
    }

    /// <summary>
    /// Enumerates all configured connectors.
    /// </summary>
    public IEnumerable<KeyValuePair<string, IConnector>> Enumerate()
    {
        return _connectors.AsEnumerable();
    }
}
