namespace DioRed.Vermilion.Interaction.Receivers;

/// <summary>
/// Represents a target set of chats for outbound bot messages.
/// </summary>
public abstract class Receiver
{
    /// <summary>
    /// Creates a receiver that targets a single chat.
    /// </summary>
    public static Receiver Chat(ChatId chatId) => new SingleChatReceiver { ChatId = chatId };

    /// <summary>
    /// Creates a broadcast receiver using the specified predicate.
    /// </summary>
    public static Receiver Where(Func<ChatMetadata, bool> filter) => Broadcast(filter);

    /// <summary>
    /// Creates a broadcast receiver using the specified predicate.
    /// </summary>
    public static Receiver Broadcast(Func<ChatMetadata, bool> filter) => new BroadcastReceiver { Filter = filter };

    /// <summary>
    /// Creates a broadcast receiver for chats that have the specified tag.
    /// </summary>
    public static Receiver WithTag(string tag) => Broadcast(chat => chat.HasTag(tag));

    /// <summary>
    /// Creates a broadcast receiver for chats that do not have the specified tag.
    /// </summary>
    public static Receiver WithoutTag(string tag) => Broadcast(chat => !chat.HasTag(tag));

    /// <summary>
    /// Creates a broadcast receiver for chats that have all specified tags.
    /// </summary>
    public static Receiver WithAllTags(params string[] tags) => Broadcast(chat => tags.All(chat.HasTag));

    /// <summary>
    /// Creates a broadcast receiver for chats that have at least one specified tag.
    /// </summary>
    public static Receiver WithAnyTag(params string[] tags) => Broadcast(chat => tags.Any(chat.HasTag));

    /// <summary>
    /// Gets a receiver that targets all known chats.
    /// </summary>
    public static Receiver Everyone { get; } = new EveryoneReceiver();

    /// <summary>
    /// Converts a chat id to a single-chat receiver.
    /// </summary>
    public static implicit operator Receiver(ChatId chatId) => Chat(chatId);
}
