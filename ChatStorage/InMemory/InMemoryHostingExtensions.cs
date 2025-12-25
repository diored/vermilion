using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Hosting;

public static class InMemoryHostingExtensions
{
    extension(IChatStorageCollection chatStorageCollection)
    {
        public void UseInMemory()
        {
            chatStorageCollection.Use(new InMemoryChatStorage());
        }
    }
}