using DioRed.Vermilion.Handling.Templates;

namespace DioRed.Vermilion.Handling;

/// <summary>
/// Describes how a command is matched and when it is allowed to run.
/// </summary>
public class CommandDefinition
{
    /// <summary>
    /// Gets the command-matching template.
    /// </summary>
    public required Template Template { get; init; }

    /// <summary>
    /// Gets the tail policy required for the command.
    /// </summary>
    public TailPolicy TailPolicy { get; init; } = TailPolicy.Any;

    /// <summary>
    /// Gets the minimum sender role required to run the command.
    /// </summary>
    public UserRole RequiredRole { get; init; } = UserRole.Member;

    /// <summary>
    /// Gets the handler priority used during matching.
    /// </summary>
    public CommandPriority Priority { get; init; } = CommandPriority.Medium;

    /// <summary>
    /// Gets a value indicating whether successful handling should be logged.
    /// </summary>
    public bool LogHandling { get; init; } = false;

    /// <summary>
    /// Gets the clients policy required for the command.
    /// </summary>
    public CommandClientsPolicy ClientsPolicy { get; init; } = CommandClientsPolicy.EligibleOnly;

    /// <summary>
    /// Determines whether the definition matches the specified runtime conditions.
    /// </summary>
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
                CommandClientsPolicy.Any => true,
                CommandClientsPolicy.EligibleOnly => clientIsEligible,
                _ => throw new ArgumentOutOfRangeException(nameof(ClientsPolicy), ClientsPolicy, null)
            };
    }
}
