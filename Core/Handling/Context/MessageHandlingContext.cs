namespace DioRed.Vermilion.Handling.Context;

/// <summary>
/// Aggregates chat, message, and sender information for a handler invocation.
/// </summary>
public class MessageHandlingContext
{
    /// <summary>
    /// Gets the chat context.
    /// </summary>
    public required ChatContext Chat { get; init; }

    /// <summary>
    /// Gets the message context.
    /// </summary>
    public required MessageContext Message { get; init; }

    /// <summary>
    /// Gets the sender context.
    /// </summary>
    public required SenderContext Sender { get; init; }
}
