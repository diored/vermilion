
using DioRed.Vermilion.Connectors;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Configures the connector set used by a Vermilion bot.
/// </summary>
public interface IConnectorsCollection
{
    /// <summary>
    /// Adds connectors produced by the specified factory.
    /// </summary>
    IConnectorsCollection Add(Func<IServiceProvider, IEnumerable<KeyValuePair<string, IConnector>>> factory);

    /// <summary>
    /// Adds a connector produced by the specified factory.
    /// </summary>
    IConnectorsCollection Add(Func<IServiceProvider, KeyValuePair<string, IConnector>> factory);

    /// <summary>
    /// Adds a connector under the specified key.
    /// </summary>
    IConnectorsCollection Add(string key, IConnector connector);

    /// <summary>
    /// Adds multiple connectors.
    /// </summary>
    IConnectorsCollection AddRange(params IEnumerable<KeyValuePair<string, IConnector>> connectors);
}
