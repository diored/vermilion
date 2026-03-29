namespace DioRed.Vermilion.Messages;

/// <summary>
/// Contains reusable exception message templates used by Vermilion components.
/// </summary>
public static class ExceptionMessages
{
    /// <summary>Message for repeated bot core initialization.</summary>
    public const string BotCoreAlreadyInitialized_0 = "Bot core already initialized";

    /// <summary>Message for using the bot core before initialization.</summary>
    public const string BotCoreNotInitialized_0 = "Bot core not initialized";

    /// <summary>Message for starting the bot core from an invalid state.</summary>
    public const string CannotStartBotCoreInState_1 = "Cannot start bot core in state {0}";

    /// <summary>Message for stopping the bot core from an invalid state.</summary>
    public const string CannotStopBotCoreInState_1 = "Cannot stop bot core in state {0}";

    /// <summary>Message for unexpected connector post results.</summary>
    public const string UnexpectedPostResult_1 = "Unexpected PostResult: {0}";

    /// <summary>Message for missing or invalid configuration values.</summary>
    public const string CannotReadConfiguration_1 = """Cannot read "{0}" value from the configuration""";

    /// <summary>Message for repeated chat storage initialization.</summary>
    public const string ChatStorageAlreadyInitialized_0 = "Chat storage already initialized";

    /// <summary>Message for invalid types used in typed registration APIs.</summary>
    public const string TypeDoesntImplementTheInterface_2 = "Type {0} doesn't implement the interface {1}";

    /// <summary>Message for repeated command handler initialization.</summary>
    public const string CommandHandlersAlreadyInitialized_0 = "Command handlers already initialized";

    /// <summary>Message for empty command handler collections.</summary>
    public const string CommandHandlersCannotBeEmpty_0 = "Command handlers cannot be empty";

    /// <summary>Message for using command handlers before initialization.</summary>
    public const string CommandHandlersShouldBeInitialized_0 = "Command handlers should be initialized";

    /// <summary>Message for repeated connector initialization.</summary>
    public const string ConnectorsAlreadyInitialized_0 = "Connectors already initialized";

    /// <summary>Message for empty connector collections.</summary>
    public const string ConnectorsCannotBeEmpty_0 = "Connectors cannot be empty";

    /// <summary>Message for using connectors before initialization.</summary>
    public const string ConnectorsShouldBeInitialized_0 = "Connectors should be initialized";

    /// <summary>Message for repeated scheduled job initialization.</summary>
    public const string ScheduledJobsAlreadyInitialized_0 = "Daily jobs already initialized";

    /// <summary>Message for missing chat storage registration.</summary>
    public const string ChatStorageShouldBeInitialized_0 = "Chat storage should be initialized";

    /// <summary>Message for missing chat client manager initialization.</summary>
    public const string ChatClientsManagerShouldBeInitialized_0 = "Chat clients manager should be initialized";

    /// <summary>Message for missing bot options initialization.</summary>
    public const string BotOptionsShouldBeInitialized_0 = "Bot options should be initialized";

    /// <summary>Message for missing clients policy initialization.</summary>
    public const string ClientsPolicyShouldBeInitialized_0 = "Clients policy should be initialized";
}
