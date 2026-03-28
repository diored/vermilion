using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Tests.ChatStorage;

public class JsonFileChatStorageTests
{
    [Test]
    public async Task AddChatAsync_Throws_WhenSameChatIdIsAddedTwice()
    {
        string filePath = CreateTempFilePath();

        try
        {
            JsonFileChatStorage storage = new(filePath);
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
        finally
        {
            DeleteIfExists(filePath);
            DeleteIfExists(filePath + ".tmp");
        }
    }

    [Test]
    public async Task PersistsChatsWithSameNumericIdButDifferentType()
    {
        string filePath = CreateTempFilePath();

        try
        {
            ChatId privateChatId = new("telegram", "Private", 42);
            ChatId groupChatId = new("telegram", "Group", 42);

            JsonFileChatStorage writer = new(filePath);
            await writer.AddChatAsync(new ChatMetadata
            {
                ChatId = privateChatId,
                Tags = ["private"]
            });
            await writer.AddChatAsync(new ChatMetadata
            {
                ChatId = groupChatId,
                Tags = ["group"]
            });

            JsonFileChatStorage reader = new(filePath);
            ChatMetadata[] chats = await reader.GetChatsAsync().ToArrayAsync();

            using var _ = Assert.Multiple();
            await Assert.That(chats).Count().IsEqualTo(2);
            await Assert.That(chats.Select(x => x.ChatId)).Contains(privateChatId);
            await Assert.That(chats.Select(x => x.ChatId)).Contains(groupChatId);
        }
        finally
        {
            DeleteIfExists(filePath);
            DeleteIfExists(filePath + ".tmp");
        }
    }

    [Test]
    public async Task UpdateChatAsync_OnlyUpdatesMatchingFullChatId_AfterReload()
    {
        string filePath = CreateTempFilePath();

        try
        {
            ChatId privateChatId = new("telegram", "Private", 42);
            ChatId groupChatId = new("telegram", "Group", 42);

            JsonFileChatStorage writer = new(filePath);
            await writer.AddChatAsync(new ChatMetadata
            {
                ChatId = privateChatId,
                Tags = ["private-original"]
            });
            await writer.AddChatAsync(new ChatMetadata
            {
                ChatId = groupChatId,
                Tags = ["group-original"]
            });

            JsonFileChatStorage updater = new(filePath);
            await updater.UpdateChatAsync(new ChatMetadata
            {
                ChatId = privateChatId,
                Tags = ["private-updated"]
            });

            JsonFileChatStorage reader = new(filePath);
            ChatMetadata privateChat = await reader.GetChatAsync(privateChatId);
            ChatMetadata groupChat = await reader.GetChatAsync(groupChatId);

            using var _ = Assert.Multiple();
            await Assert.That(privateChat.Tags).Contains("private-updated");
            await Assert.That(groupChat.Tags).Contains("group-original");
        }
        finally
        {
            DeleteIfExists(filePath);
            DeleteIfExists(filePath + ".tmp");
        }
    }

    private static string CreateTempFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "vermilion-tests",
            $"{Guid.NewGuid():N}.json"
        );
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
