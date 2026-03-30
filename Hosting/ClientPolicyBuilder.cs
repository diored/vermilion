using Rule = System.Func<DioRed.Vermilion.ChatId, bool>;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Legacy alias preserved for migration from older Vermilion versions.
/// </summary>
[Obsolete("Use BotVisibilityBuilder instead.")]
public class ClientPolicyBuilder : BotVisibilityBuilder
{
    /// <summary>
    /// Legacy alias for <see cref="BotVisibilityBuilder.Public"/>.
    /// </summary>
    public ClientPolicyBuilder AllowForEveryone()
    {
        Public();
        return this;
    }

    /// <summary>
    /// Legacy alias for <see cref="BotVisibilityBuilder.PrivateTo(IEnumerable{ChatId})"/>.
    /// </summary>
    public ClientPolicyBuilder AllowFor(params IEnumerable<ChatId> chatIds)
    {
        PrivateTo(chatIds);
        return this;
    }

    /// <summary>
    /// Legacy alias for <see cref="BotVisibilityBuilder.PrivateTo(Rule)"/>.
    /// </summary>
    public ClientPolicyBuilder AllowFor(Rule condition)
    {
        PrivateTo(condition);
        return this;
    }

    /// <summary>
    /// Legacy alias for <see cref="BotVisibilityBuilder.HiddenFromEveryone"/>.
    /// </summary>
    public ClientPolicyBuilder DenyForEveryone()
    {
        HiddenFromEveryone();
        return this;
    }

    /// <summary>
    /// Legacy alias for <see cref="BotVisibilityBuilder.HiddenFrom(IEnumerable{ChatId})"/>.
    /// </summary>
    public ClientPolicyBuilder DenyFor(params IEnumerable<ChatId> chatIds)
    {
        HiddenFrom(chatIds);
        return this;
    }

    /// <summary>
    /// Legacy alias for <see cref="BotVisibilityBuilder.HiddenFrom(Rule)"/>.
    /// </summary>
    public ClientPolicyBuilder DenyFor(Rule condition)
    {
        HiddenFrom(condition);
        return this;
    }

    /// <summary>
    /// Builds the resulting clients policy.
    /// </summary>
    public new ClientsPolicy Build()
    {
        return new ClientsPolicy(base.Build().IsVisibleFor);
    }
}
