namespace DioRed.Vermilion.Interaction.Content;

/// <summary>
/// Image content stored in memory as a byte array.
/// </summary>
public class ImageBytesContent : IContent
{
    /// <summary>
    /// Gets the raw image bytes.
    /// </summary>
    public required byte[] Content { get; init; }
}
