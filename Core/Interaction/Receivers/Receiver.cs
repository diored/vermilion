namespace DioRed.Vermilion.Interaction.Receivers;

public abstract class Receiver
{
    public static Receiver Chat(ChatId chatId) => new SingleChatReceiver { ChatId = chatId };
    public static Receiver Broadcast(Func<ChatMetadata, bool> filter) => new BroadcastReceiver { Filter = filter };
    public static Receiver Everyone { get; } = new EveryoneReceiver();

    public static implicit operator Receiver(ChatId chatId) => Chat(chatId);
}