using Telegram.Bot.Types;

namespace DioRed.Vermilion.Events;

public class ChatEventArgs : EventArgs
{
    public ChatEventArgs(Chat chat)
    {
        Chat = chat;
    }

    public Chat Chat { get; }
}
