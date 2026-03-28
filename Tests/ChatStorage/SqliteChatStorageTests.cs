using DioRed.Vermilion.ChatStorage;

using Microsoft.Data.Sqlite;

namespace DioRed.Vermilion.Tests.ChatStorage;

public class SqliteChatStorageTests
{
    [Test]
    public async Task AddChatAsync_Throws_WhenSameChatIdIsAddedTwice()
    {
        string databasePath = CreateTempFilePath();

        try
        {
            SqliteChatStorage storage = new($"Data Source={databasePath}");
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
            DeleteIfExists(databasePath);
        }
    }

    [Test]
    public async Task SupportsDistinctChatTypesForSameNumericId()
    {
        string databasePath = CreateTempFilePath();

        try
        {
            SqliteChatStorage storage = new($"Data Source={databasePath}");
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

            ChatMetadata[] chats = await storage.GetChatsAsync().ToArrayAsync();

            using var _ = Assert.Multiple();
            await Assert.That(chats).Count().IsEqualTo(2);
            await Assert.That(chats.Select(x => x.ChatId)).Contains(privateChatId);
            await Assert.That(chats.Select(x => x.ChatId)).Contains(groupChatId);
        }
        finally
        {
            DeleteIfExists(databasePath);
        }
    }

    [Test]
    public async Task MigratesLegacySchemaAndUsesTypeAwarePrimaryKey()
    {
        string databasePath = CreateTempFilePath();

        try
        {
            await CreateLegacySchemaAsync(databasePath).ConfigureAwait(false);
            await InsertLegacyRowAsync(databasePath).ConfigureAwait(false);

            SqliteChatStorage storage = new($"Data Source={databasePath}");

            ChatMetadata privateChat = await storage.GetChatAsync(new ChatId("telegram", "Private", 42));
            await storage.AddChatAsync(new ChatMetadata
            {
                ChatId = new ChatId("telegram", "Group", 42),
                Tags = ["group"]
            });

            ChatMetadata groupChat = await storage.GetChatAsync(new ChatId("telegram", "Group", 42));

            using var _ = Assert.Multiple();
            await Assert.That(privateChat.Tags).Contains("legacy");
            await Assert.That(groupChat.Tags).Contains("group");
        }
        finally
        {
            DeleteIfExists(databasePath);
        }
    }

    private static async Task CreateLegacySchemaAsync(string databasePath)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE [Chats] (
                [System] TEXT NOT NULL,
                [Id] INTEGER NOT NULL,
                [Type] TEXT NOT NULL,
                [Title] TEXT NOT NULL,
                PRIMARY KEY ([System], [Id])
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertLegacyRowAsync(string databasePath)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using SqliteCommand addTags = connection.CreateCommand();
        addTags.CommandText = """ALTER TABLE [Chats] ADD COLUMN [Tags] TEXT NULL;""";
        await addTags.ExecuteNonQueryAsync();

        await using SqliteCommand insert = connection.CreateCommand();
        insert.CommandText =
            """
            INSERT INTO [Chats] ([System], [Id], [Type], [Title], [Tags])
            VALUES ($system, $id, $type, $title, $tags);
            """;
        insert.Parameters.AddWithValue("$system", "telegram");
        insert.Parameters.AddWithValue("$id", 42L);
        insert.Parameters.AddWithValue("$type", "Private");
        insert.Parameters.AddWithValue("$title", string.Empty);
        insert.Parameters.AddWithValue("$tags", "[\"legacy\"]");
        await insert.ExecuteNonQueryAsync();
    }

    private static string CreateTempFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "vermilion-tests",
            $"{Guid.NewGuid():N}.db"
        );
    }

    private static void DeleteIfExists(string path)
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        string walPath = path + "-wal";
        if (File.Exists(walPath))
        {
            File.Delete(walPath);
        }

        string shmPath = path + "-shm";
        if (File.Exists(shmPath))
        {
            File.Delete(shmPath);
        }
    }
}
