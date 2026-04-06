namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Describes the outcome of a storage-to-storage chat migration.
/// </summary>
public sealed class ChatStorageMigrationResult
{
    /// <summary>
    /// Gets the number of chats copied to the target storage.
    /// </summary>
    public required int MigratedChats { get; init; }
}
