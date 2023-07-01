namespace DioRed.Vermilion;

public interface IChatWriter
{
    Task SendHtmlAsync(string html);
    Task SendPhotoAsync(string url, bool allowCaching = true);
    Task SendPhotoAsync(Stream stream);
    Task SendTextAsync(string text);
}