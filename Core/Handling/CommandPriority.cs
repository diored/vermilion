namespace DioRed.Vermilion.Handling;

/// <summary>
/// Defines the relative priority of a command handler.
/// </summary>
public enum CommandPriority : byte
{
    /// <summary>
    /// Lowest possible priority.
    /// </summary>
    Lowest = 0,
    /// <summary>
    /// Low priority.
    /// </summary>
    Low = 63,
    /// <summary>
    /// Default priority.
    /// </summary>
    Medium = 127,
    /// <summary>
    /// High priority.
    /// </summary>
    High = 191,
    /// <summary>
    /// Highest possible priority.
    /// </summary>
    Highest = 255
}
