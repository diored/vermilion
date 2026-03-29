namespace DioRed.Vermilion.Interaction.Receivers;

/// <summary>
/// Receiver that targets a single chat.
/// </summary>
public class SingleChatReceiver : Receiver
{
    /// <summary>
    /// Gets the target chat identity.
    /// </summary>
    public required ChatId ChatId { get; init; }
}
