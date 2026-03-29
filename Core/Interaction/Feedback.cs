using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Interaction.Receivers;

namespace DioRed.Vermilion.Interaction;

/// <summary>
/// Helper API used by handlers and jobs to send content and mutate chat tags.
/// </summary>
public class Feedback(
    BotCore botCore,
    Receiver receiver,
    CancellationToken ct = default
)
{
    /// <summary>
    /// Sends plain text to the current receiver.
    /// </summary>
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

    /// <summary>
    /// Sends HTML content to the current receiver.
    /// </summary>
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

    /// <summary>
    /// Sends an image from a byte array to the current receiver.
    /// </summary>
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

    /// <summary>
    /// Sends an image from a stream to the current receiver.
    /// </summary>
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

    /// <summary>
    /// Sends an image from a URL to the current receiver.
    /// </summary>
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

    /// <summary>
    /// Builds and sends content per target chat.
    /// </summary>
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

    /// <summary>
    /// Sends prebuilt content to the current receiver.
    /// </summary>
    public Task ContentAsync(
        IContent content,
        CancellationToken overrideCt = default
    )
    {
        return botCore.PostAsync(receiver, content, ResolveCt(overrideCt));
    }

    /// <summary>
    /// Builds and sends content per target chat.
    /// </summary>
    public Task ContentAsync(
        Func<ChatMetadata, IContent> contentBuilder,
        CancellationToken overrideCt = default
    )
    {
        return botCore.PostAsync(receiver, contentBuilder, ResolveCt(overrideCt));
    }

    /// <summary>
    /// Builds and sends content per target chat.
    /// </summary>
    public Task ContentAsync(
        Func<ChatMetadata, Task<IContent>> contentBuilder,
        CancellationToken overrideCt = default
    )
    {
        return ContentAsync((chatMetadata, _) => contentBuilder(chatMetadata), overrideCt);
    }

    /// <summary>
    /// Adds a tag to all chats matched by the current receiver.
    /// </summary>
    public async Task AddTagAsync(string tag, CancellationToken overrideCt = default)
    {
        await botCore.AddTagAsync(
            receiver,
            tag,
            ResolveCt(overrideCt)
        );
    }


    /// <summary>
    /// Removes a tag from all chats matched by the current receiver.
    /// </summary>
    public async Task RemoveTagAsync(string tag, CancellationToken overrideCt = default)
    {
        await botCore.RemoveTagAsync(
            receiver,
            tag,
            ResolveCt(overrideCt)
        );
    }

    /// <summary>
    /// Returns a feedback helper that targets a single chat.
    /// </summary>
    public Feedback To(ChatId chatId) => new(botCore, Receiver.Chat(chatId), ct);

    /// <summary>
    /// Returns a feedback helper that targets chats matching the specified predicate.
    /// </summary>
    public Feedback ToWhere(Func<ChatMetadata, bool> filter) => new(botCore, Receiver.Where(filter), ct);

    /// <summary>
    /// Returns a feedback helper that targets chats matching the specified predicate.
    /// </summary>
    public Feedback To(Func<ChatMetadata, bool> filter) => new(botCore, Receiver.Broadcast(filter), ct);

    /// <summary>
    /// Returns a feedback helper that targets chats with the specified tag.
    /// </summary>
    public Feedback ToWithTag(string tag) => new(botCore, Receiver.WithTag(tag), ct);

    /// <summary>
    /// Returns a feedback helper that targets chats without the specified tag.
    /// </summary>
    public Feedback ToWithoutTag(string tag) => new(botCore, Receiver.WithoutTag(tag), ct);

    /// <summary>
    /// Returns a feedback helper that targets chats with all specified tags.
    /// </summary>
    public Feedback ToWithAllTags(params string[] tags) => new(botCore, Receiver.WithAllTags(tags), ct);

    /// <summary>
    /// Returns a feedback helper that targets chats with any of the specified tags.
    /// </summary>
    public Feedback ToWithAnyTag(params string[] tags) => new(botCore, Receiver.WithAnyTag(tags), ct);

    /// <summary>
    /// Returns a feedback helper that targets all known chats.
    /// </summary>
    public Feedback ToEveryone() => new(botCore, Receiver.Everyone, ct);

    private CancellationToken ResolveCt(CancellationToken overrideCt)
    {
        return overrideCt == default ? ct : overrideCt;
    }
}
