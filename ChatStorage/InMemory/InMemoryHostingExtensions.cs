using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Adds in-memory chat storage registration helpers.
/// </summary>
public static class InMemoryHostingExtensions
{
    extension(IChatStorageCollection chatStorageCollection)
    {
        /// <summary>
        /// Uses the in-memory chat storage implementation.
        /// </summary>
        public void UseInMemory()
        {
            chatStorageCollection.Use(new InMemoryChatStorage());
        }
    }

}
