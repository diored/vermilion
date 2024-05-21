using DioRed.Vermilion.Handling.Templates;

namespace DioRed.Vermilion.Handling;

public class CommandDefinition
{
    public required Template Template { get; init; }
    public bool? HasTail { get; init; }
    public UserRole RequiredRole { get; init; } = UserRole.Member;
    public CommandPriority Priority { get; init; } = CommandPriority.Medium;
    public bool LogHandling { get; init; } = false;
    public bool EligibleClientsOnly { get; init; } = true;

    public bool Matches(
        string command,
        bool hasTail,
        UserRole senderRole,
        bool clientIsEligible
    )
    {
        return Template.Matches(command) &&
            (
                !HasTail.HasValue ||
                HasTail == hasTail
            ) &&
            senderRole.HasFlag(RequiredRole) &&
            (
                !EligibleClientsOnly ||
                clientIsEligible
            );
    }
}