using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;

namespace DioRed.Vermilion.Handling;

public class SimpleCommandHandler(
    CommandDefinition commandDefinition,
    Func<MessageHandlingContext, Feedback, Task<bool>> handle
) : ICommandHandler
{
    public SimpleCommandHandler(
        CommandDefinition commandDefinition,
        Func<MessageHandlingContext, Feedback, Task> handle
    ) : this(
            commandDefinition,
            async (context, feedback) =>
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
        Feedback send
    )
    {
        return await handle(context, send);
    }
}