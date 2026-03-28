using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;

namespace DioRed.Vermilion.Handling;

public class SimpleCommandHandler(
    CommandDefinition commandDefinition,
    Func<MessageHandlingContext, Feedback, CancellationToken, Task<bool>> handle
) : ICommandHandler
{
    public SimpleCommandHandler(
        CommandDefinition commandDefinition,
        Func<MessageHandlingContext, Feedback, Task<bool>> handle
    ) : this(
            commandDefinition,
            (context, feedback, _) => handle(context, feedback)
        )
    {
    }

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

    public CommandDefinition Definition => commandDefinition;

    public async Task<bool> HandleAsync(
        MessageHandlingContext context,
        Feedback send,
        CancellationToken ct = default
    )
    {
        return await handle(context, send, ct);
    }
}
