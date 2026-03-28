namespace DioRed.Vermilion.ChatStorage;

public class SqliteChatStorageOptions
{
    public required string ConnectionString { get; init; }
    public string TableName { get; set; } = Defaults.TableName;
}
