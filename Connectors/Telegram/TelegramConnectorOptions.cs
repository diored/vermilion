namespace DioRed.Vermilion.Connectors.Telegram;

public class TelegramConnectorOptions
{
    public required string BotToken { get; init; }
    public long[] SuperAdmins { get; set; } = [];
    public string ConnectorKey { get; set; } = Defaults.ConnectorKey;
}