namespace DioRed.Vermilion;

public class Broadcaster : IChatWriter
{
    private readonly Bot _bot;

    public Broadcaster(Bot bot)
    {
        _bot = bot;
    }

    public event Action<Exception>? OnException;

    public async Task SendHtmlAsync(string html)
    {
        await _bot.Broadcast(writer => writer.SendHtmlAsync(html));
    }

    public async Task SendPhotoAsync(string url)
    {
        await _bot.Broadcast(writer => writer.SendPhotoAsync(url));
    }

    public async Task SendPhotoAsync(Stream stream)
    {
        await _bot.Broadcast(writer => writer.SendPhotoAsync(stream));
    }

    public async Task SendTextAsync(string text)
    {
        await _bot.Broadcast(writer => writer.SendTextAsync(text));
    }
}