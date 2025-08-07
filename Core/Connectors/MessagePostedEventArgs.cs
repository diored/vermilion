namespace DioRed.Vermilion.Connectors;

public class MessagePostedEventArgs : EventArgs
{
    public required int MessageId { get; init; }
    public required string Message { get; init; }
    public required ChatId ChatId { get; init; }
    public required string ChatTitle { get; init; }
    public required long SenderId { get; init; }
    public required UserRole SenderRole { get; init; }
    public required string SenderName { get; init; }
}