namespace DioRed.Vermilion;

/// <summary>
/// Legacy visibility API preserved for migration from older Vermilion versions.
/// </summary>
[Obsolete("Use BotVisibility instead.")]
public class ClientsPolicy : BotVisibility
{
    private readonly Func<ChatId, bool> _isAllowed;

    /// <summary>
    /// Initializes a legacy clients policy wrapper.
    /// </summary>
    public ClientsPolicy(Func<ChatId, bool> isAllowed) : base(isAllowed)
    {
        _isAllowed = isAllowed;
    }

    /// <summary>
    /// Evaluates whether the specified chat is eligible.
    /// </summary>
    public bool IsEligible(ChatId chatId) => _isAllowed(chatId);

    /// <summary>
    /// Gets a policy that allows every chat.
    /// </summary>
    public static ClientsPolicy All { get; } = new(_ => true);

    /// <summary>
    /// Gets a policy that denies every chat.
    /// </summary>
    public static ClientsPolicy None { get; } = new(_ => false);

    /// <summary>
    /// Creates a policy that allows only the specified chats.
    /// </summary>
    public static ClientsPolicy Whitelist(IEnumerable<ChatId> allowed)
    {
        HashSet<ChatId> allowedSet = [.. allowed];
        return new(chatId => allowedSet.Contains(chatId));
    }

    /// <summary>
    /// Creates a policy that denies only the specified chats.
    /// </summary>
    public static ClientsPolicy Blacklist(IEnumerable<ChatId> disallowed)
    {
        HashSet<ChatId> disallowedSet = [.. disallowed];
        return new(chatId => !disallowedSet.Contains(chatId));
    }
}
