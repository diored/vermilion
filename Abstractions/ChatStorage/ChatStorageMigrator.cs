using System.Runtime.CompilerServices;

namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Copies chats from one storage provider to another.
/// </summary>
public static class ChatStorageMigrator
{
    /// <summary>
    /// Copies every chat from <paramref name="source"/> into <paramref name="target"/>.
    /// The target storage must be empty before migration starts.
    /// </summary>
    public static async Task<ChatStorageMigrationResult> MigrateAsync(
        IChatStorage source,
        IChatStorage target,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (!await IsEmptyAsync(target, ct).ConfigureAwait(false))
        {
            throw new StorageMigrationException(
                "Target chat storage must be empty before migration starts."
            );
        }

        int migratedChats = 0;

        await foreach (StoredChatRecord storedChat in ExportChatsAsync(source, ct).ConfigureAwait(false))
        {
            await target.AddChatAsync(storedChat.Metadata, storedChat.Title, ct).ConfigureAwait(false);
            migratedChats++;
        }

        return new ChatStorageMigrationResult
        {
            MigratedChats = migratedChats
        };
    }

    private static async Task<bool> IsEmptyAsync(IChatStorage target, CancellationToken ct)
    {
        await foreach (ChatMetadata _ in target.GetChatsAsync(ct).ConfigureAwait(false))
        {
            return false;
        }

        return true;
    }

    private static async IAsyncEnumerable<StoredChatRecord> ExportChatsAsync(
        IChatStorage storage,
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        if (storage is IChatStorageExport export)
        {
            await foreach (StoredChatRecord storedChat in export.ExportChatsAsync(ct).ConfigureAwait(false))
            {
                yield return storedChat;
            }

            yield break;
        }

        await foreach (ChatMetadata metadata in storage.GetChatsAsync(ct).ConfigureAwait(false))
        {
            yield return new StoredChatRecord
            {
                Metadata = metadata
            };
        }
    }
}
