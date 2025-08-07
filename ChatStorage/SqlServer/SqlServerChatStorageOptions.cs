namespace DioRed.Vermilion.ChatStorage;
public class SqlServerChatStorageOptions
{
    public required string ConnectionString { get; init; }
    public string TableName { get; set; } = Defaults.TableName;
    public string Schema { get; set; } = Defaults.Schema;
}