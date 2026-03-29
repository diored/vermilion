namespace DioRed.Vermilion.Handling.Context;

/// <summary>
/// Describes the message being handled.
/// </summary>
public class MessageContext
{
    /// <summary>
    /// Gets the platform-specific message identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the full message text.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the parsed command token.
    /// </summary>
    public required string Command { get; init; }

    /// <summary>
    /// Gets the raw tail text after the command.
    /// </summary>
    public required string Tail { get; init; }

    /// <summary>
    /// Gets the parsed message arguments.
    /// </summary>
    public required MessageArgs Args { get; init; }
}
