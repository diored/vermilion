using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;

namespace DioRed.Vermilion.Handling;

public class SimpleCommandHandler(
    CommandDefinition commandDefinition,
    Func<MessageHandlingContext, Feedback, Task> handle
) : ICommandHandler
{
    public CommandDefinition Definition => commandDefinition;

    public async Task<bool> HandleAsync(
        MessageHandlingContext context,
        Feedback send
    )
    {
        await handle(context, send);

        return true;
    }
}