namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Exposes persisted chat records for storage-to-storage migration scenarios.
/// </summary>
public interface IChatStorageExport
{
    /// <summary>
    /// Streams all persisted chat records, including storage-only fields such as titles.
    /// </summary>
    IAsyncEnumerable<StoredChatRecord> ExportChatsAsync(CancellationToken ct = default);
}
