namespace DioRed.Vermilion;

/// <summary>
/// Represents the lifecycle state of <see cref="BotCore"/>.
/// </summary>
public enum BotCoreState
{
    /// <summary>
    /// The bot has not been initialized yet.
    /// </summary>
    NotInitialized,
    /// <summary>
    /// The bot is loading state and wiring dependencies.
    /// </summary>
    Initializing,
    /// <summary>
    /// The bot is initialized but not yet receiving messages.
    /// </summary>
    Initialized,
    /// <summary>
    /// The bot is starting connectors and runtime infrastructure.
    /// </summary>
    Starting,
    /// <summary>
    /// The bot is running.
    /// </summary>
    Started,
    /// <summary>
    /// The bot is stopping connectors and runtime infrastructure.
    /// </summary>
    Stopping,
    /// <summary>
    /// The bot is stopped.
    /// </summary>
    Stopped
}
