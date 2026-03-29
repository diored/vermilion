namespace DioRed.Vermilion.Interaction.Receivers;

public abstract class Receiver
{
    public static Receiver Chat(ChatId chatId) => new SingleChatReceiver { ChatId = chatId };
    public static Receiver Where(Func<ChatMetadata, bool> filter) => Broadcast(filter);
    public static Receiver Broadcast(Func<ChatMetadata, bool> filter) => new BroadcastReceiver { Filter = filter };
    public static Receiver WithTag(string tag) => Broadcast(chat => chat.HasTag(tag));
    public static Receiver WithoutTag(string tag) => Broadcast(chat => !chat.HasTag(tag));
    public static Receiver WithAllTags(params string[] tags) => Broadcast(chat => tags.All(chat.HasTag));
    public static Receiver WithAnyTag(params string[] tags) => Broadcast(chat => tags.Any(chat.HasTag));
    public static Receiver Everyone { get; } = new EveryoneReceiver();

    public static implicit operator Receiver(ChatId chatId) => Chat(chatId);
}
