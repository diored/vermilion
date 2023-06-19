namespace DioRed.Vermilion;

public abstract class MessageContext<TChatClient, TBot> : IMessageContext
    where TChatClient : IChatClient
    where TBot : VermilionBot
{
    public required int MessageId { get; init; }
    public required UserRole Role { get; init; }
    public required TChatClient ChatClient { get; init; }
    public required TBot Bot { get; init; }
    public required CancellationToken CancellationToken { get; init; }

    public IChatWriter GetChatWriter()
    {
        return Bot.GetChatWriter(ChatClient.ChatId);
    }
}