namespace DioRed.Vermilion;

public class ChatClient
{
    public required ChatMetadata Metadata { get; init; }
    public Dictionary<string, object?> RuntimeValues { get; init; } = [];
}