namespace DioRed.Vermilion.Interaction.Receivers;

public class BroadcastReceiver : Receiver
{
    public required Func<ChatId, bool> Filter { get; init; }
}