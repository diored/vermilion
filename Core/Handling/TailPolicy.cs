namespace DioRed.Vermilion.Handling;

/// <summary>
/// Describes whether a command requires a tail.
/// </summary>
public enum TailPolicy
{
    /// <summary>
    /// Presence of a tail is not required, but it can be present.
    /// </summary>
    Any,

    /// <summary>
    /// Presence of a tail is required.
    /// </summary>
    HasTail,

    /// <summary>
    /// Presence of a tail is not allowed.
    /// </summary>
    HasNoTail
}