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
    /// Gets or sets the visibility rule that determines which chats may interact with the bot.
    /// </summary>
    public BotVisibility Visibility { get; init; } = BotVisibility.Public;

    /// <summary>
    /// Legacy alias preserved for migration from older Vermilion versions.
    /// </summary>
    [Obsolete("Use Visibility instead.")]
    public ClientsPolicy? ClientsPolicy
    {
        init => Visibility = value ?? BotVisibility.Public;
    }
}
