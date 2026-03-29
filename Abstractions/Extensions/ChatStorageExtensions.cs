using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Extensions;

public static class ChatStorageExtensions
{
    extension(IChatStorage chatStorage)
    {
        public async Task<ChatMetadata[]> GetChatsArrayAsync(CancellationToken ct = default)
        {
            List<ChatMetadata> chats = [];

            await foreach (ChatMetadata chat in chatStorage.GetChatsAsync(ct))
            {
                chats.Add(chat);
            }

            return [.. chats];
        }

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
