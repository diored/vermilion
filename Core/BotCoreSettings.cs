using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Connectors;
using DioRed.Vermilion.Handling;

namespace DioRed.Vermilion;

/// <summary>
/// Collects all services and policies required to construct <see cref="BotCore"/>.
/// </summary>
public class BotCoreSettings
{
    /// <summary>
    /// Gets the chat storage used by the bot.
    /// </summary>
    public required IChatStorage ChatStorage { get; init; }

    /// <summary>
    /// Gets the connectors used by the bot.
    /// </summary>
    public required ICollection<KeyValuePair<string, IConnector>> Connectors { get; init; }

    /// <summary>
    /// Gets the command handlers used by the bot.
    /// </summary>
    public required ICollection<ICommandHandler> CommandHandlers { get; init; }

    /// <summary>
    /// Gets or sets the runtime chat client manager.
    /// </summary>
    public ChatClientsManager ChatClientsManager { get; init; } = new ChatClientsManager();

    /// <summary>
    /// Gets or sets optional runtime behavior.
    /// </summary>
    public BotOptions Options { get; init; } = new();

    /// <summary>
    /// Gets or sets the policy that determines which chats are eligible for command handling.
    /// </summary>
    public ClientsPolicy ClientsPolicy { get; init; } = ClientsPolicy.All;
}
