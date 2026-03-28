using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Tests.ChatStorage;

public class InMemoryChatStorageTests
{
    [Test]
    public async Task AddChatAsync_Throws_WhenSameChatIdIsAddedTwice()
    {
        InMemoryChatStorage storage = new();
        ChatId chatId = new("telegram", "Private", 42);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = chatId,
            Tags = ["first"]
        });

        await Assert.That(async () => await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = chatId,
            Tags = ["second"]
        })).Throws<ChatAlreadyExistsException>();
    }

    [Test]
    public async Task GetChatsAsync_KeepsChatsWithSameNumericIdButDifferentType()
    {
        InMemoryChatStorage storage = new();

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = new ChatId("telegram", "Private", 42),
            Tags = ["private"]
        });

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = new ChatId("telegram", "Group", 42),
            Tags = ["group"]
        });

        ChatMetadata[] chats = await storage.GetChatsAsync().ToArrayAsync();

        using var _ = Assert.Multiple();
        await Assert.That(chats).Count().IsEqualTo(2);
        await Assert.That(chats.Select(x => x.ChatId))
            .Contains(new ChatId("telegram", "Private", 42));
        await Assert.That(chats.Select(x => x.ChatId))
            .Contains(new ChatId("telegram", "Group", 42));
    }

    [Test]
    public async Task UpdateChatAsync_OnlyUpdatesMatchingFullChatId()
    {
        InMemoryChatStorage storage = new();
        ChatId privateChatId = new("telegram", "Private", 42);
        ChatId groupChatId = new("telegram", "Group", 42);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = privateChatId,
            Tags = ["private-original"]
        });

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = groupChatId,
            Tags = ["group-original"]
        });

        await storage.UpdateChatAsync(new ChatMetadata
        {
            ChatId = privateChatId,
            Tags = ["private-updated"]
        });

        ChatMetadata privateChat = await storage.GetChatAsync(privateChatId);
        ChatMetadata groupChat = await storage.GetChatAsync(groupChatId);

        using var _ = Assert.Multiple();
        await Assert.That(privateChat.Tags).Contains("private-updated");
        await Assert.That(groupChat.Tags).Contains("group-original");
    }
}
