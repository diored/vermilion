using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Interaction.Receivers;

namespace DioRed.Vermilion.Interaction;

public class Feedback(
    BotCore botCore,
    Receiver receiver,
    CancellationToken ct = default
)
{
    public async Task TextAsync(string text, CancellationToken overrideCt = default)
    {
        await botCore.PostAsync(
            receiver,
            new TextContent
            {
                Text = text
            },
            ResolveCt(overrideCt)
        );
    }

    public async Task HtmlAsync(string html, CancellationToken overrideCt = default)
    {
        await botCore.PostAsync(
            receiver,
            new HtmlContent
            {
                Html = html
            },
            ResolveCt(overrideCt)
        );
    }

    public async Task ImageAsync(byte[] content, CancellationToken overrideCt = default)
    {
        await botCore.PostAsync(
            receiver,
            new ImageBytesContent
            {
                Content = content
            },
            ResolveCt(overrideCt)
        );
    }

    public async Task ImageAsync(Stream stream, CancellationToken overrideCt = default)
    {
        await botCore.PostAsync(
            receiver,
            new ImageStreamContent
            {
                Stream = stream
            },
            ResolveCt(overrideCt)
        );
    }

    public async Task ImageAsync(string url, CancellationToken overrideCt = default)
    {
        await botCore.PostAsync(
            receiver,
            new ImageUrlContent
            {
                Url = url
            },
            ResolveCt(overrideCt)
        );
    }

    public async Task ContentAsync(
        Func<ChatMetadata, CancellationToken, Task<IContent>> contentBuilder,
        CancellationToken overrideCt = default
    )
    {
        CancellationToken effectiveCt = ResolveCt(overrideCt);
        await botCore.PostAsync(
            receiver,
            chatMetadata => contentBuilder(chatMetadata, effectiveCt),
            effectiveCt
        );
    }

    public Task ContentAsync(
        Func<ChatMetadata, Task<IContent>> contentBuilder,
        CancellationToken overrideCt = default
    )
    {
        return ContentAsync((chatMetadata, _) => contentBuilder(chatMetadata), overrideCt);
    }

    public async Task AddTagAsync(string tag, CancellationToken overrideCt = default)
    {
        await botCore.AddTagAsync(
            receiver,
            tag,
            ResolveCt(overrideCt)
        );
    }


    public async Task RemoveTagAsync(string tag, CancellationToken overrideCt = default)
    {
        await botCore.RemoveTagAsync(
            receiver,
            tag,
            ResolveCt(overrideCt)
        );
    }

    public Feedback To(ChatId chatId) => new(botCore, Receiver.Chat(chatId), ct);
    public Feedback To(Func<ChatMetadata, bool> filter) => new(botCore, Receiver.Broadcast(filter), ct);
    public Feedback ToEveryone() => new(botCore, Receiver.Everyone, ct);

    private CancellationToken ResolveCt(CancellationToken overrideCt)
    {
        return overrideCt == default ? ct : overrideCt;
    }
}
