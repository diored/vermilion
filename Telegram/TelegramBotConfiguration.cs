namespace DioRed.Vermilion.Telegram;

public class TelegramBotConfiguration
{
    public required string BotToken { get; init; }
    public long? SuperAdminId { get; init; }
}