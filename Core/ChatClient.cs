namespace DioRed.Vermilion;

/// <summary>
/// Holds persisted metadata and mutable runtime values for a chat.
/// </summary>
public class ChatClient
{
    /// <summary>
    /// Gets the immutable chat metadata.
    /// </summary>
    public required ChatMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the mutable runtime values associated with the chat.
    /// </summary>
    public Dictionary<string, object?> RuntimeValues { get; init; } = [];
}
