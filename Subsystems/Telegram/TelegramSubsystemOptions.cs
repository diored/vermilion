namespace DioRed.Vermilion.Subsystems.Telegram;

public class TelegramSubsystemOptions
{
    public required string BotToken { get; init; }
    public long[] SuperAdmins { get; init; } = [];
}