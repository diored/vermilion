namespace DioRed.Vermilion.Handling;

public class CommandHandlersManager(IEnumerable<ICommandHandler> commandHandlers)
{
    private readonly List<ICommandHandler> _commandHandlers = [..commandHandlers];

    public ICommandHandler[] FindMatchedHandlers(
        string command,
        bool hasTail,
        UserRole senderRole,
        bool clientIsEligible
    )
    {
        return
        [
            .. _commandHandlers
                .Where(handler => handler.Definition.Matches(
                        command,
                        hasTail,
                        senderRole,
                        clientIsEligible
                ))
                .OrderByDescending(
                    handler => handler.Definition.Priority
                )
        ];
    }
}