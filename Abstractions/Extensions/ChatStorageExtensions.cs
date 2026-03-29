using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Extensions;

/// <summary>
/// Convenience helpers for working with <see cref="IChatStorage"/> streaming APIs.
/// </summary>
public static class ChatStorageExtensions
{
    extension(IChatStorage chatStorage)
    {
        /// <summary>
        /// Materializes all chats into an array.
        /// </summary>
        public async Task<ChatMetadata[]> GetChatsArrayAsync(CancellationToken ct = default)
        {
            List<ChatMetadata> chats = [];

            await foreach (ChatMetadata chat in chatStorage.GetChatsAsync(ct))
            {
                chats.Add(chat);
            }

            return [.. chats];
        }

        /// <summary>
        /// Materializes all chats into a list.
        /// </summary>
        public async Task<List<ChatMetadata>> GetChatsListAsync(CancellationToken ct = default)
        {
            List<ChatMetadata> chats = [];

            await foreach (ChatMetadata chat in chatStorage.GetChatsAsync(ct))
            {
                chats.Add(chat);
            }

            return chats;
        }
    }
}
