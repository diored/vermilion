namespace DioRed.Vermilion;

public class ClientsPolicy(Func<ChatId, bool> isAllowed)
{
    public bool IsEligible(ChatId chatId) => isAllowed(chatId);

    public static ClientsPolicy All { get; } = new(_ => true);
    public static ClientsPolicy None { get; } = new(_ => false);

    public static ClientsPolicy Whitelist(IEnumerable<ChatId> allowed)
    {
        HashSet<ChatId> allowedSet = [.. allowed];
        return new(chatId => allowedSet.Contains(chatId));
    }

    public static ClientsPolicy Blacklist(IEnumerable<ChatId> disallowed)
    {
        HashSet<ChatId> disallowedSet = [.. disallowed];
        return new(chatId => !disallowedSet.Contains(chatId));
    }
}