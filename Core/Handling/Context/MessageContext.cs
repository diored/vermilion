namespace DioRed.Vermilion.Handling.Context;

public class MessageContext
{
    public required int Id { get; init; }
    public required string Text { get; init; }
    public required string Command { get; init; }
    public required string Tail { get; init; }
    public required MessageArgs Args { get; init; }
}