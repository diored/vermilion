namespace DioRed.Vermilion.Handling.Context;

public class MessageHandlingContext
{
    public required ChatContext Chat { get; init; }
    public required MessageContext Message { get; init; }
    public required SenderContext Sender { get; init; }
}