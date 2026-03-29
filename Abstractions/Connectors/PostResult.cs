namespace DioRed.Vermilion.Connectors;

/// <summary>
/// Represents the outcome of an outbound connector send attempt.
/// </summary>
public enum PostResult
{
    /// <summary>
    /// The message was sent successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The connector does not support the requested content type.
    /// </summary>
    ContentTypeNotSupported,

    /// <summary>
    /// The connector can no longer access the destination chat.
    /// </summary>
    ChatAccessDenied,

    /// <summary>
    /// The connector failed due to a platform or transport issue.
    /// </summary>
    ConnectorFailure,

    /// <summary>
    /// The connector hit an unexpected internal error.
    /// </summary>
    UnexpectedException
}
