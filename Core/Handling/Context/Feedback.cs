using DioRed.Vermilion.Interaction.Content;
using DioRed.Vermilion.Interaction.Receivers;

namespace DioRed.Vermilion.Handling.Context;

public class Feedback(
    BotCore botCore,
    Receiver receiver
)
{
    public async Task TextAsync(string text)
    {
        await botCore.PostAsync(
            receiver,
            new TextContent
            {
                Text = text
            }
        );
    }

    public async Task HtmlAsync(string html)
    {
        await botCore.PostAsync(
            receiver,
            new HtmlContent
            {
                Html = html
            }
        );
    }

    public async Task ImageAsync(byte[] content)
    {
        await botCore.PostAsync(
            receiver,
            new ImageBytesContent
            {
                Content = content
            }
        );
    }

    public async Task ImageAsync(Stream stream)
    {
        await botCore.PostAsync(
            receiver,
            new ImageStreamContent
            {
                Stream = stream
            }
        );
    }

    public async Task ImageAsync(string url)
    {
        await botCore.PostAsync(
            receiver,
            new ImageUrlContent
            {
                Url = url
            }
        );
    }

    public Feedback To(ChatId chatId) => new(botCore, Receiver.Chat(chatId));
    public Feedback To(Func<ChatId, bool> filter) => new(botCore, Receiver.Broadcast(filter));
    public Feedback ToEveryone() => new(botCore, Receiver.Everyone);
}