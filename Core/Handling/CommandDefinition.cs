using DioRed.Vermilion.Handling.Templates;

namespace DioRed.Vermilion.Handling;

public class CommandDefinition
{
    public required Template Template { get; init; }
    public TailPolicy TailPolicy { get; init; } = TailPolicy.Any;
    public UserRole RequiredRole { get; init; } = UserRole.Member;
    public CommandPriority Priority { get; init; } = CommandPriority.Medium;
    public bool LogHandling { get; init; } = false;
    public ClientsPolicy ClientsPolicy { get; init; } = ClientsPolicy.EligibleOnly;

    public bool Matches(
        string command,
        bool hasTail,
        UserRole senderRole,
        bool clientIsEligible
    )
    {
        return Template.Matches(command) &&
            TailPolicy switch
            {
                TailPolicy.Any => true,
                TailPolicy.HasTail => hasTail,
                TailPolicy.HasNoTail => !hasTail,
                _ => throw new ArgumentOutOfRangeException(nameof(TailPolicy), TailPolicy, null)
            } &&
            senderRole.HasFlag(RequiredRole) &&
            ClientsPolicy switch
            {
                ClientsPolicy.Any => true,
                ClientsPolicy.EligibleOnly => clientIsEligible,
                _ => throw new ArgumentOutOfRangeException(nameof(ClientsPolicy), ClientsPolicy, null)
            };
    }
}