namespace DioRed.Vermilion.Interaction.Receivers;

public class BroadcastReceiver : Receiver
{
    public required Func<ChatInfo, bool> Filter { get; init; }
}