namespace DioRed.Vermilion;

public record struct ChatId(string System, string Type, long Id)
{
    public override readonly string ToString()
    {
        return $"{System} {Type} chat #{Id}";
    }
}