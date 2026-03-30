using Rule = System.Func<DioRed.Vermilion.ChatId, bool>;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Builds a <see cref="BotVisibility"/> by composing visibility and hidden rules.
/// </summary>
public class BotVisibilityBuilder
{
    private Rule? _policy;

    /// <summary>
    /// Makes the bot visible to every chat.
    /// </summary>
    public BotVisibilityBuilder Public()
    {
        AddRule(chatId => true);
        return this;
    }

    /// <summary>
    /// Makes the bot visible only to the specified chats.
    /// </summary>
    public BotVisibilityBuilder PrivateTo(params IEnumerable<ChatId> chatIds)
    {
        HashSet<ChatId> chatIdSet = [.. chatIds];
        AddRule(chatIdSet.Contains);
        return this;
    }

    /// <summary>
    /// Makes the bot visible only to chats that satisfy the specified condition.
    /// </summary>
    public BotVisibilityBuilder PrivateTo(Rule condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        AddRule(condition);
        return this;
    }

    /// <summary>
    /// Hides the bot from every chat.
    /// </summary>
    public BotVisibilityBuilder HiddenFromEveryone()
    {
        AddRule(chatId => false);
        return this;
    }

    /// <summary>
    /// Hides the bot from the specified chats.
    /// </summary>
    public BotVisibilityBuilder HiddenFrom(params IEnumerable<ChatId> chatIds)
    {
        HashSet<ChatId> chatIdSet = [.. chatIds];
        AddRule(chatId => !chatIdSet.Contains(chatId));
        return this;
    }

    /// <summary>
    /// Hides the bot from chats that satisfy the specified condition.
    /// </summary>
    public BotVisibilityBuilder HiddenFrom(Rule condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        AddRule(chatId => !condition(chatId));
        return this;
    }

    /// <summary>
    /// Builds the resulting bot visibility rule.
    /// </summary>
    public BotVisibility Build()
    {
        return _policy is not null
            ? new BotVisibility(_policy)
            : BotVisibility.Public;
    }

    private void AddRule(Rule rule)
    {
        if (_policy is null)
        {
            _policy = rule;
        }
        else
        {
            var previous = _policy;
            _policy = chatId => previous(chatId) && rule(chatId);
        }
    }
}
