namespace DioRed.Vermilion;

/// <summary>
/// Configures optional runtime behavior for <see cref="BotCore"/>.
/// </summary>
public class BotOptions
{
    /// <summary>
    /// Gets or sets a custom greeting written to logs during startup.
    /// </summary>
    public string? Greeting { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether chat titles should be stored when available.
    /// </summary>
    public bool SaveChatTitles { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether handled commands should be logged.
    /// </summary>
    public bool LogCommands { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the core version should be logged at startup.
    /// </summary>
    public bool ShowCoreVersion { get; set; } = true;
}
