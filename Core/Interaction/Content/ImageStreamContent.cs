namespace DioRed.Vermilion.Interaction.Content;

public class ImageStreamContent : IContent
{
    public required Stream Stream { get; init; }
}