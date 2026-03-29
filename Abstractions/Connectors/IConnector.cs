using DioRed.Vermilion.Interaction.Content;

namespace DioRed.Vermilion.Connectors;

/// <summary>
/// Adapter contract between Vermilion and a specific messaging platform.
/// </summary>
public interface IConnector
{
    /// <summary>
    /// Raised when the connector receives a message that should be handled by the bot.
    /// </summary>
    event EventHandler<MessagePostedEventArgs>? MessagePosted;

    /// <summary>
    /// Gets the connector implementation version when available.
    /// </summary>
    string? Version => null;

    /// <summary>
    /// Starts the connector and begins receiving updates.
    /// </summary>
    Task StartAsync(CancellationToken ct);

    /// <summary>
    /// Stops the connector and releases platform-specific resources.
    /// </summary>
    Task StopAsync(CancellationToken ct);

    /// <summary>
    /// Sends content to the specified platform-specific chat identifier.
    /// </summary>
    Task<PostResult> PostAsync(long internalId, IContent content, CancellationToken ct = default);

    /// <summary>
    /// Determines whether the specified chat identity belongs to a configured super-admin.
    /// </summary>
    bool IsSuperAdmin(ChatId chatId);
}
