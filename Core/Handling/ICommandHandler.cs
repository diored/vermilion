using DioRed.Vermilion.Handling.Context;
using DioRed.Vermilion.Interaction;

namespace DioRed.Vermilion.Handling;

public interface ICommandHandler
{
    CommandDefinition Definition { get; }
    Task<bool> HandleAsync(MessageHandlingContext context, Feedback feedback);
}