namespace DioRed.Vermilion.Interaction.Content;

/// <summary>
/// Image content provided as a stream.
/// </summary>
public class ImageStreamContent : IContent
{
    /// <summary>
    /// Gets the image stream.
    /// </summary>
    public required Stream Stream { get; init; }
}
