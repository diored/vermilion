namespace DioRed.Vermilion;

/// <summary>
/// Identifies a chat within a specific connector and chat type.
/// </summary>
public record struct ChatId(string ConnectorKey, string Type, long Id)
{
    /// <summary>
    /// Returns a readable representation of the chat identity.
    /// </summary>
    public override readonly string ToString()
    {
        return $"{ConnectorKey} {Type} chat #{Id}";
    }
}
