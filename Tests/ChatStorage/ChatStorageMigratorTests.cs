using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Tests.ChatStorage;

public sealed class ChatStorageMigratorTests
{
    [Test]
    public async Task MigrateAsync_CopiesChatsIntoEmptyTarget_AndPreservesTitles()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "vermilion-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string sourcePath = Path.Combine(tempDir, "source.json");
            string targetPath = Path.Combine(tempDir, "target.json");

            JsonFileChatStorage source = new(sourcePath);
            JsonFileChatStorage target = new(targetPath);

            await source.AddChatAsync(
                new ChatMetadata
                {
                    ChatId = new ChatId("telegram", "private", 42),
                    Tags = ["vip", "alpha"]
                },
                "Alice"
            );

            ChatStorageMigrationResult result = await ChatStorageMigrator.MigrateAsync(source, target);
            StoredChatRecord exported = await GetSingleExportedChatAsync(target);

            await Assert.That(result.MigratedChats).IsEqualTo(1);
            await Assert.That(exported.Metadata.ChatId).IsEqualTo(new ChatId("telegram", "private", 42));
            await Assert.That(exported.Metadata.Tags.OrderBy(x => x).ToArray()).IsEquivalentTo(new[] { "alpha", "vip" });
            await Assert.That(exported.Title).IsEqualTo("Alice");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public async Task MigrateAsync_Fails_WhenTargetIsNotEmpty()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "vermilion-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string sourcePath = Path.Combine(tempDir, "source.json");
            string targetPath = Path.Combine(tempDir, "target.json");

            JsonFileChatStorage source = new(sourcePath);
            JsonFileChatStorage target = new(targetPath);

            await source.AddChatAsync(
                new ChatMetadata
                {
                    ChatId = new ChatId("telegram", "private", 1),
                    Tags = []
                },
                "Source"
            );

            await target.AddChatAsync(
                new ChatMetadata
                {
                    ChatId = new ChatId("telegram", "private", 2),
                    Tags = []
                },
                "Target"
            );

            await Assert.That(async () => await ChatStorageMigrator.MigrateAsync(source, target))
                .Throws<StorageMigrationException>();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static async Task<StoredChatRecord> GetSingleExportedChatAsync(IChatStorage storage)
    {
        IChatStorageExport export = (IChatStorageExport)storage;

        await foreach (StoredChatRecord storedChat in export.ExportChatsAsync())
        {
            return storedChat;
        }

        throw new InvalidOperationException("Expected one exported chat.");
    }
}
