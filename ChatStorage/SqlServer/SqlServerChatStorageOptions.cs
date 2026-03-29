namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Configures the SQL Server chat storage provider.
/// </summary>
public class SqlServerChatStorageOptions
{
    /// <summary>
    /// Gets the SQL Server connection string.
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Gets or sets the table name that stores chat metadata.
    /// </summary>
    public string TableName { get; set; } = Defaults.TableName;

    /// <summary>
    /// Gets or sets the schema that contains the chat table.
    /// </summary>
    public string Schema { get; set; } = Defaults.Schema;
}
