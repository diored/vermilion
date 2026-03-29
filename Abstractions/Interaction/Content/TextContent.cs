namespace DioRed.Vermilion.Interaction.Content;

/// <summary>
/// Plain text content.
/// </summary>
public class TextContent : IContent
{
    /// <summary>
    /// Gets the text payload.
    /// </summary>
    public required string Text { get; init; }
}
