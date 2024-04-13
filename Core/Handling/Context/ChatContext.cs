namespace DioRed.Vermilion.Handling.Context;

public class ChatContext
{
    public required ChatId Id { get; init; }
    public required Dictionary<string, object?> Properties { get; init; } = [];
}