using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Hosting;

public static class InMemoryHostingExtensions
{
    public static void UseInMemory(
        this ChatStorageCollection chatStorageCollection
    )
    {
        chatStorageCollection.Use(new InMemoryChatStorage());
    }
}