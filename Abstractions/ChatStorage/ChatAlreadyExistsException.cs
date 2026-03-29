namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Thrown when a storage provider is asked to add a chat that already exists.
/// </summary>
public class ChatAlreadyExistsException : Exception
{
    /// <summary>
    /// Initializes a new exception for the specified chat.
    /// </summary>
    public ChatAlreadyExistsException(ChatId chatId)
        : base($"Chat {chatId} is already stored.")
    {
        ChatId = chatId;
    }

    /// <summary>
    /// Initializes a new exception for the specified chat and inner exception.
    /// </summary>
    public ChatAlreadyExistsException(ChatId chatId, Exception innerException)
        : base($"Chat {chatId} is already stored.", innerException)
    {
        ChatId = chatId;
    }

    /// <summary>
    /// Gets the chat identity that caused the conflict.
    /// </summary>
    public ChatId ChatId { get; }
}
