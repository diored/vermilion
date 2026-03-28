using DioRed.Vermilion.ChatStorage;

using MongoDB.Bson;
using MongoDB.Driver;

using Testcontainers.MongoDb;

namespace DioRed.Vermilion.Tests.Integration;

[RequiresDocker]
[NotInParallel("Docker")]
[Property("Category", "Integration")]
public class MongoDbChatStorageIntegrationTests
{
    private const string DatabaseName = "vermilion_tests";
    private static MongoDbContainer? _container;

    [Before(Class)]
    public static async Task StartContainer()
    {
        _container = new MongoDbBuilder("mongo:7.0").Build();
        await Container.StartAsync().ConfigureAwait(false);
    }

    [After(Class)]
    public static async Task StopContainer()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
            _container = null;
        }
    }

    [Test]
    public async Task SupportsDistinctChatTypesForSameNumericId()
    {
        string collectionName = UniqueCollectionName();
        MongoDbChatStorage storage = CreateStorage(collectionName);

        ChatId privateChatId = new("telegram", "Private", 42);
        ChatId groupChatId = new("telegram", "Group", 42);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = privateChatId,
            Tags = ["private"]
        });
        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = groupChatId,
            Tags = ["group"]
        });

        ChatMetadata privateChat = await storage.GetChatAsync(privateChatId);
        ChatMetadata groupChat = await storage.GetChatAsync(groupChatId);

        using var _ = Assert.Multiple();
        await Assert.That(privateChat.Tags).Contains("private");
        await Assert.That(groupChat.Tags).Contains("group");
    }

    [Test]
    public async Task MigratesLegacyDocumentsMissingTags()
    {
        string collectionName = UniqueCollectionName();
        IMongoCollection<BsonDocument> collection = CreateRawCollection(collectionName);

        await collection.InsertOneAsync(new BsonDocument
        {
            { "system", "telegram" },
            { "type", "Private" },
            { "id", 42L }
        });

        MongoDbChatStorage storage = CreateStorage(collectionName);
        ChatMetadata chat = await storage.GetChatAsync(new ChatId("telegram", "Private", 42));

        await storage.UpdateChatAsync(new ChatMetadata
        {
            ChatId = new ChatId("telegram", "Private", 42),
            Tags = ["migrated"]
        });

        ChatMetadata updatedChat = await storage.GetChatAsync(new ChatId("telegram", "Private", 42));

        using var _ = Assert.Multiple();
        await Assert.That(chat.Tags).Count().IsEqualTo(0);
        await Assert.That(updatedChat.Tags).Contains("migrated");
    }

    private static MongoDbChatStorage CreateStorage(string collectionName)
    {
        return new MongoDbChatStorage(
            Container.GetConnectionString(),
            DatabaseName,
            collectionName
        );
    }

    private static IMongoCollection<BsonDocument> CreateRawCollection(string collectionName)
    {
        MongoClient client = new(Container.GetConnectionString());
        return client.GetDatabase(DatabaseName).GetCollection<BsonDocument>(collectionName);
    }

    private static string UniqueCollectionName() => $"chats_{Guid.NewGuid():N}";

    private static MongoDbContainer Container =>
        _container ?? throw new InvalidOperationException("MongoDB container has not been started.");
}
