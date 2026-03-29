namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Configures the SQLite chat storage provider.
/// </summary>
public class SqliteChatStorageOptions
{
    /// <summary>
    /// Gets the SQLite connection string.
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Gets or sets the table name that stores chat metadata.
    /// </summary>
    public string TableName { get; set; } = Defaults.TableName;
}
