using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Handling;

namespace DioRed.Vermilion;

public class BotCoreSettings
{
    public required IChatStorage ChatStorage { get; init; }
    public required ICollection<KeyValuePair<string, IConnector>> Connectors { get; init; }
    public required ICollection<ICommandHandler> CommandHandlers { get; init; }
    public ChatClientsManager ChatClientsManager { get; init; } = new ChatClientsManager();
    public BotOptions Options { get; init; } = new();
    public ClientsPolicy ClientsPolicy { get; init; } = ClientsPolicy.All;
}