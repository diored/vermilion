using Rule = System.Func<DioRed.Vermilion.ChatId, bool>;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Builds a <see cref="ClientsPolicy"/> by composing allow and deny rules.
/// </summary>
public class ClientPolicyBuilder
{
    private Rule? _policy;

    /// <summary>
    /// Allows messages to be sent to every chat.
    /// </summary>
    public ClientPolicyBuilder AllowForEveryone()
    {
        AddRule(chatId => true);

        return this;
    }

    /// <summary>
    /// Allows messages to be sent only to the specified chats.
    /// </summary>
    public ClientPolicyBuilder AllowFor(params IEnumerable<ChatId> chatIds)
    {
        HashSet<ChatId> chatIdSet = [.. chatIds];

        AddRule(chatIdSet.Contains);

        return this;
    }

    /// <summary>
    /// Allows messages to be sent only to chats that satisfy the specified condition.
    /// </summary>
    public ClientPolicyBuilder AllowFor(Rule condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        AddRule(condition);

        return this;
    }

    /// <summary>
    /// Denies messages for every chat.
    /// </summary>
    public ClientPolicyBuilder DenyForEveryone()
    {
        AddRule(chatId => false);

        return this;
    }

    /// <summary>
    /// Denies messages for the specified chats.
    /// </summary>
    public ClientPolicyBuilder DenyFor(params IEnumerable<ChatId> chatIds)
    {
        HashSet<ChatId> chatIdSet = [.. chatIds];

        AddRule(chatId => !chatIdSet.Contains(chatId));

        return this;
    }

    /// <summary>
    /// Denies messages for chats that satisfy the specified condition.
    /// </summary>
    public ClientPolicyBuilder DenyFor(Rule condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        AddRule(chatId => !condition(chatId));

        return this;
    }

    /// <summary>
    /// Builds the resulting clients policy.
    /// </summary>
    public ClientsPolicy Build()
    {
        return _policy is not null
            ? new ClientsPolicy(_policy)
            : ClientsPolicy.All;
    }

    private void AddRule(Rule rule)
    {
        if (_policy is null)
        {
            _policy = rule;
        }
        else
        {
            // IMPORTANT: capture the previous policy delegate.
            // If we reference _policy directly in the new lambda, it becomes self-referential
            // after assignment and will cause a stack overflow at runtime.
            var previous = _policy;
            _policy = chatId => previous(chatId) && rule(chatId);
        }
    }
}
