namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Thrown when a storage provider cannot complete its schema or data migration.
/// </summary>
public class StorageMigrationException : Exception
{
    /// <summary>
    /// Initializes a new exception with the specified migration error message.
    /// </summary>
    public StorageMigrationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new exception with the specified migration error message and inner exception.
    /// </summary>
    public StorageMigrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
