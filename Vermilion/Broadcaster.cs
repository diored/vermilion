namespace DioRed.Vermilion;

public class Broadcaster : IChatWriter
{
    private readonly VermilionManager _botManager;

    public Broadcaster(VermilionManager botManager)
    {
        _botManager = botManager;
    }

    public async Task SendHtmlAsync(string html)
    {
        await _botManager.Broadcast(writer => writer.SendHtmlAsync(html));
    }

    public async Task SendPhotoAsync(string url)
    {
        await _botManager.Broadcast(writer => writer.SendPhotoAsync(url));
    }

    public async Task SendPhotoAsync(Stream stream)
    {
        await _botManager.Broadcast(writer => writer.SendPhotoAsync(stream));
    }

    public async Task SendTextAsync(string text)
    {
        await _botManager.Broadcast(writer => writer.SendTextAsync(text));
    }
}