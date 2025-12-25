namespace DioRed.Vermilion;

public record struct ChatId(string ConnectorKey, string Type, long Id)
{
    public override readonly string ToString()
    {
        return $"{ConnectorKey} {Type} chat #{Id}";
    }
}