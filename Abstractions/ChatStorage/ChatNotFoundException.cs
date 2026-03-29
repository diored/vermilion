namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Thrown when a storage provider cannot find the requested chat.
/// </summary>
public class ChatNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new exception for the specified missing chat.
    /// </summary>
    public ChatNotFoundException(ChatId chatId)
        : base($"Chat {chatId} not found.")
    {
        ChatId = chatId;
    }

    /// <summary>
    /// Gets the missing chat identity.
    /// </summary>
    public ChatId ChatId { get; }
}
