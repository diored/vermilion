namespace DioRed.Vermilion.Interaction.Receivers;

public class BroadcastReceiver : Receiver
{
    public required Func<ChatMetadata, bool> Filter { get; init; }
}