using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Configures the Azure Table chat storage provider.
/// </summary>
public class AzureTableChatStorageOptions
{
    /// <summary>
    /// Gets the Azure Storage connection settings used by the provider.
    /// </summary>
    public required AzureStorageSettings Settings { get; init; }

    /// <summary>
    /// Gets or sets the table name that stores chat metadata.
    /// </summary>
    public string TableName { get; set; } = Defaults.TableName;
}
