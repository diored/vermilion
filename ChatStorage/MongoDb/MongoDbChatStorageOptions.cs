namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Configures the MongoDB chat storage provider.
/// </summary>
public class MongoDbChatStorageOptions
{
    /// <summary>
    /// Gets the MongoDB connection string.
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Gets or sets the database name that stores chat metadata.
    /// </summary>
    public string DatabaseName { get; set; } = Defaults.DatabaseName;

    /// <summary>
    /// Gets or sets the collection name that stores chat metadata.
    /// </summary>
    public string CollectionName { get; set; } = Defaults.CollectionName;
}
