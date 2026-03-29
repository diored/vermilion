namespace DioRed.Vermilion.Connectors;

/// <summary>
/// Describes a message received from a connector.
/// </summary>
public class MessagePostedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the platform-specific message identifier.
    /// </summary>
    public required int MessageId { get; init; }

    /// <summary>
    /// Gets the text payload that Vermilion should process.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the chat identity that received the message.
    /// </summary>
    public required ChatId ChatId { get; init; }

    /// <summary>
    /// Gets a human-readable chat title when available.
    /// </summary>
    public required string ChatTitle { get; init; }

    /// <summary>
    /// Gets the platform-specific sender identifier.
    /// </summary>
    public required long SenderId { get; init; }

    /// <summary>
    /// Gets the sender role resolved by the connector.
    /// </summary>
    public required UserRole SenderRole { get; init; }

    /// <summary>
    /// Gets the sender display name when available.
    /// </summary>
    public required string SenderName { get; init; }
}
