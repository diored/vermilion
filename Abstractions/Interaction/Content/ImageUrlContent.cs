namespace DioRed.Vermilion.Interaction.Content;

/// <summary>
/// Image content referenced by URL.
/// </summary>
public class ImageUrlContent : IContent
{
    /// <summary>
    /// Gets the image URL.
    /// </summary>
    public required string Url { get; init; }
}
