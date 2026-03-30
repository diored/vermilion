namespace DioRed.Vermilion;

/// <summary>
/// Determines which chats the bot is visible to for command handling and outbound interaction.
/// </summary>
public class BotVisibility(Func<ChatId, bool> isVisible)
{
    /// <summary>
    /// Evaluates whether the bot is visible to the specified chat.
    /// </summary>
    public bool IsVisibleFor(ChatId chatId) => isVisible(chatId);

    /// <summary>
    /// Gets a visibility rule that allows every chat.
    /// </summary>
    public static BotVisibility Public { get; } = new(_ => true);

    /// <summary>
    /// Gets a visibility rule that hides the bot from every chat.
    /// </summary>
    public static BotVisibility Hidden { get; } = new(_ => false);

    /// <summary>
    /// Creates a visibility rule that allows only the specified chats.
    /// </summary>
    public static BotVisibility PrivateTo(IEnumerable<ChatId> allowed)
    {
        HashSet<ChatId> allowedSet = [.. allowed];
        return new(chatId => allowedSet.Contains(chatId));
    }

    /// <summary>
    /// Creates a visibility rule that hides the bot only from the specified chats.
    /// </summary>
    public static BotVisibility HiddenFrom(IEnumerable<ChatId> disallowed)
    {
        HashSet<ChatId> disallowedSet = [.. disallowed];
        return new(chatId => !disallowedSet.Contains(chatId));
    }
}
