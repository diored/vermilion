using DioRed.Vermilion.Handling.Context;

namespace DioRed.Vermilion.Handling;

public interface ICommandHandler
{
    CommandDefinition Definition { get; }
    Task<bool> HandleAsync(MessageHandlingContext context, Feedback feedback);
}