using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;

namespace DioRed.Vermilion.Handling;

/// <summary>
/// Lightweight command handler wrapper built from delegates.
/// </summary>
public class SimpleCommandHandler(
    CommandDefinition commandDefinition,
    Func<MessageHandlingContext, Feedback, CancellationToken, Task<bool>> handle
) : ICommandHandler
{
    /// <summary>
    /// Initializes a handler from an asynchronous delegate that returns whether handling succeeded.
    /// </summary>
    public SimpleCommandHandler(
        CommandDefinition commandDefinition,
        Func<MessageHandlingContext, Feedback, Task<bool>> handle
    ) : this(
            commandDefinition,
            (context, feedback, _) => handle(context, feedback)
        )
    {
    }

    /// <summary>
    /// Initializes a handler from an asynchronous delegate that always counts as handled on success.
    /// </summary>
    public SimpleCommandHandler(
        CommandDefinition commandDefinition,
        Func<MessageHandlingContext, Feedback, CancellationToken, Task> handle
    ) : this(
            commandDefinition,
            async (context, feedback, ct) =>
            {
                await handle(context, feedback, ct);

                return true;
            }
        )
    {
    }

    /// <summary>
    /// Initializes a handler from an asynchronous delegate that always counts as handled on success.
    /// </summary>
    public SimpleCommandHandler(
        CommandDefinition commandDefinition,
        Func<MessageHandlingContext, Feedback, Task> handle
    ) : this(
            commandDefinition,
            async (context, feedback, _) =>
            {
                await handle(context, feedback);

                return true;
            }
        )
    {
    }

    /// <summary>
    /// Gets the command definition for this handler.
    /// </summary>
    public CommandDefinition Definition => commandDefinition;

    /// <summary>
    /// Executes the wrapped delegate.
    /// </summary>
    public async Task<bool> HandleAsync(
        MessageHandlingContext context,
        Feedback send,
        CancellationToken ct = default
    )
    {
        return await handle(context, send, ct);
    }
}
