
using DioRed.Vermilion.Connectors;

namespace DioRed.Vermilion.Hosting;

public interface IConnectorsCollection
{
    IConnectorsCollection Add(Func<IServiceProvider, IEnumerable<KeyValuePair<string, IConnector>>> factory);
    IConnectorsCollection Add(Func<IServiceProvider, KeyValuePair<string, IConnector>> factory);
    IConnectorsCollection Add(string key, IConnector connector);
    IConnectorsCollection AddRange(params IEnumerable<KeyValuePair<string, IConnector>> connectors);
}