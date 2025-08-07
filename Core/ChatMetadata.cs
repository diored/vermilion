namespace DioRed.Vermilion;

public class ChatMetadata
{
    public required ChatId ChatId { get; init; }
    public HashSet<string> Tags { get; init; } = [];

    public override string ToString()
    {
        return ChatId.ToString();
    }
}