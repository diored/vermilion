namespace DioRed.Vermilion;

/// <summary>
/// Represents the sender role as seen by a connector and the bot runtime.
/// </summary>
[Flags]
public enum UserRole
{
    /// <summary>
    /// The sender is a bot account.
    /// </summary>
    Bot = 1,

    /// <summary>
    /// The sender is a regular chat member.
    /// </summary>
    Member = 2,

    /// <summary>
    /// The sender is an administrator of the current chat.
    /// </summary>
    ChatAdmin = 4,

    /// <summary>
    /// The sender is a globally trusted bot administrator.
    /// </summary>
    SuperAdmin = 128
}
