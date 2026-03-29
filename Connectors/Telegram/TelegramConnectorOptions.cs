namespace DioRed.Vermilion.Connectors.Telegram;

/// <summary>
/// Configures the Telegram connector.
/// </summary>
public class TelegramConnectorOptions
{
    /// <summary>
    /// Gets the Telegram bot token.
    /// </summary>
    public required string BotToken { get; init; }

    /// <summary>
    /// Gets or sets the global super-admin user ids.
    /// </summary>
    public long[] SuperAdmins { get; set; } = [];

    /// <summary>
    /// Gets or sets the connector key used in <see cref="ChatId"/>.
    /// </summary>
    public string ConnectorKey { get; set; } = Defaults.ConnectorKey;
}
