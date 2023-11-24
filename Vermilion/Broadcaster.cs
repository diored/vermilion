namespace DioRed.Vermilion;

public class Broadcaster(VermilionManager botManager) : IChatWriter
{
    public async Task SendHtmlAsync(string html)
    {
        await botManager.Broadcast(writer => writer.SendHtmlAsync(html));
    }

    public async Task SendPhotoAsync(string url)
    {
        await botManager.Broadcast(writer => writer.SendPhotoAsync(url));
    }

    public async Task SendPhotoAsync(Stream stream)
    {
        await botManager.Broadcast(writer => writer.SendPhotoAsync(stream));
    }

    public async Task SendTextAsync(string text)
    {
        await botManager.Broadcast(writer => writer.SendTextAsync(text));
    }
}