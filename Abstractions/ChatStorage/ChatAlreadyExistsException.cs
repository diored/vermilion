namespace DioRed.Vermilion.ChatStorage;

public class ChatAlreadyExistsException : Exception
{
    public ChatAlreadyExistsException(ChatId chatId)
        : base($"Chat {chatId} is already stored.")
    {
        ChatId = chatId;
    }

    public ChatAlreadyExistsException(ChatId chatId, Exception innerException)
        : base($"Chat {chatId} is already stored.", innerException)
    {
        ChatId = chatId;
    }

    public ChatId ChatId { get; }
}
