namespace DioRed.Vermilion.Handling;

/// <summary>
/// Determines whether a command can run for any chat or only eligible chats.
/// </summary>
public enum CommandClientsPolicy
{
    /// <summary>
    /// The command runs only for chats allowed by the runtime clients policy.
    /// </summary>
    EligibleOnly,

    /// <summary>
    /// The command runs regardless of the runtime clients policy.
    /// </summary>
    Any
}
