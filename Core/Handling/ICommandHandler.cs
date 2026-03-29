using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;

namespace DioRed.Vermilion.Handling;

/// <summary>
/// Handles matched bot commands.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Gets the command definition used to match and order the handler.
    /// </summary>
    CommandDefinition Definition { get; }

    /// <summary>
    /// Handles a matched command.
    /// </summary>
    Task<bool> HandleAsync(MessageHandlingContext context, Feedback feedback, CancellationToken ct = default);
}
