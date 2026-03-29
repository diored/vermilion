namespace DioRed.Vermilion.Interaction.Receivers;

/// <summary>
/// Receiver that targets chats matching a predicate.
/// </summary>
public class BroadcastReceiver : Receiver
{
    /// <summary>
    /// Gets the predicate used to filter chats.
    /// </summary>
    public required Func<ChatMetadata, bool> Filter { get; init; }
}
