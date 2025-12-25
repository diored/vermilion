namespace DioRed.Vermilion.Interaction.Content;

public class ImageBytesContent : IContent
{
    public required byte[] Content { get; init; }
}