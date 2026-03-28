namespace DioRed.Vermilion.ChatStorage;

public class MongoDbChatStorageOptions
{
    public required string ConnectionString { get; init; }
    public string DatabaseName { get; set; } = Defaults.DatabaseName;
    public string CollectionName { get; set; } = Defaults.CollectionName;
}
