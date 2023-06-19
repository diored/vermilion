namespace DioRed.Vermilion;

public interface IMessageContext
{
    int MessageId { get; init; }
    UserRole Role { get; init; }
    CancellationToken CancellationToken { get; init; }

    IChatWriter GetChatWriter();
}