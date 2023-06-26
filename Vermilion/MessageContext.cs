namespace DioRed.Vermilion;

public class MessageContext
{
    public required ChatId ChatId { get; init; }
    public required int MessageId { get; init; }
    public required UserRole UserRole { get; init; }
    public required VermilionBot Bot { get; init; }

    public ChatClient ChatClient => Bot.Manager.Chats.GetClient(ChatId)
        ?? throw new InvalidOperationException($"Chat client was not found: {ChatId}");
}