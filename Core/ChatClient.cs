namespace DioRed.Vermilion;

internal class ChatClient
{
    public required ChatInfo ChatInfo { get; init; }
    public Dictionary<string, object?> Properties { get; init; } = [];
}