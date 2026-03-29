namespace DioRed.Vermilion.Interaction.Content;

/// <summary>
/// HTML content.
/// </summary>
public class HtmlContent : IContent
{
    /// <summary>
    /// Gets the HTML payload.
    /// </summary>
    public required string Html { get; init; }
}
