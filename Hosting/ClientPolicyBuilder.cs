using Rule = System.Func<DioRed.Vermilion.ChatId, bool>;

namespace DioRed.Vermilion.Hosting;
public class ClientPolicyBuilder
{
    private Rule? _policy;

    public ClientPolicyBuilder AllowForEveryone()
    {
        AddRule(chatId => true);

        return this;
    }

    public ClientPolicyBuilder AllowFor(params IEnumerable<ChatId> chatIds)
    {
        HashSet<ChatId> chatIdSet = [.. chatIds];

        AddRule(chatIdSet.Contains);

        return this;
    }

    public ClientPolicyBuilder AllowFor(Rule condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        AddRule(condition);

        return this;
    }

    public ClientPolicyBuilder DenyForEveryone()
    {
        AddRule(chatId => false);

        return this;
    }

    public ClientPolicyBuilder DenyFor(params IEnumerable<ChatId> chatIds)
    {
        HashSet<ChatId> chatIdSet = [.. chatIds];

        AddRule(chatId => !chatIdSet.Contains(chatId));

        return this;
    }

    public ClientPolicyBuilder DenyFor(Rule condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        AddRule(chatId => !condition(chatId));

        return this;
    }

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