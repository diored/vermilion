namespace DioRed.Vermilion.Handling.Context;

/// <summary>
/// Describes the message sender.
/// </summary>
public class SenderContext
{
    /// <summary>
    /// Gets the platform-specific sender identifier.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets the sender role resolved by the connector.
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Gets the sender display name when available.
    /// </summary>
    public required string Name { get; init; }
}
