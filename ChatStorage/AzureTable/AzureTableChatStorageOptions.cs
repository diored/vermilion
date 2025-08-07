using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;
public class AzureTableChatStorageOptions
{
    public required AzureStorageSettings Settings { get; init; }
    public string TableName { get; set; } = Defaults.TableName;
}