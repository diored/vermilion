namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Represents a persisted chat record, including storage-only fields that are not part of <see cref="ChatMetadata"/>.
/// </summary>
public sealed class StoredChatRecord
{
    /// <summary>
    /// Gets the chat metadata stored for the chat.
    /// </summary>
    public required ChatMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the persisted chat title when available.
    /// </summary>
    public string? Title { get; init; }
}
