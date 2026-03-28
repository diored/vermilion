namespace DioRed.Vermilion.ChatStorage;

public class ChatNotFoundException : Exception
{
    public ChatNotFoundException(ChatId chatId)
        : base($"Chat {chatId} not found.")
    {
        ChatId = chatId;
    }

    public ChatId ChatId { get; }
}
