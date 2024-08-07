namespace DioRed.Vermilion;

public class ChatInfo
{
    public required ChatId ChatId { get; init; }
    public string[] Tags { get; init; } = [];

    public override string ToString()
    {
        return ChatId.ToString();
    }
}