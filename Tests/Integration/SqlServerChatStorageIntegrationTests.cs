using System.Data;

using DioRed.Vermilion.ChatStorage;

using Microsoft.Data.SqlClient;

using Testcontainers.MsSql;

namespace DioRed.Vermilion.Tests.Integration;

[RequiresDocker]
[NotInParallel("Docker")]
[Property("Category", "Integration")]
public class SqlServerChatStorageIntegrationTests
{
    private const string SchemaName = "dbo";
    private static MsSqlContainer? _container;

    [Before(Class)]
    public static async Task StartContainer()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
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
        string tableName = UniqueTableName();
        SqlServerChatStorage storage = CreateStorage(tableName);

        ChatId privateChatId = new("telegram", "Private", 42);
        ChatId groupChatId = new("telegram", "Group", 42);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = privateChatId,
            Tags = ["private"]
        }).ConfigureAwait(false);

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = groupChatId,
            Tags = ["group"]
        }).ConfigureAwait(false);

        ChatMetadata privateChat = await storage.GetChatAsync(privateChatId).ConfigureAwait(false);
        ChatMetadata groupChat = await storage.GetChatAsync(groupChatId).ConfigureAwait(false);

        using var _ = Assert.Multiple();
        await Assert.That(privateChat.Tags).Contains("private");
        await Assert.That(groupChat.Tags).Contains("group");
    }

    [Test]
    public async Task MigratesLegacySchemaAndUsesTypeAwarePrimaryKey()
    {
        string tableName = UniqueTableName();

        await CreateLegacySchemaAsync(tableName).ConfigureAwait(false);
        await InsertLegacyRowAsync(tableName, "telegram", 42, "Private", "[\"legacy\"]").ConfigureAwait(false);

        SqlServerChatStorage storage = CreateStorage(tableName);

        ChatMetadata chat = await storage.GetChatAsync(new ChatId("telegram", "Private", 42)).ConfigureAwait(false);
        await Assert.That(chat.Tags).Contains("legacy");

        await storage.AddChatAsync(new ChatMetadata
        {
            ChatId = new ChatId("telegram", "Group", 42),
            Tags = ["group"]
        }).ConfigureAwait(false);

        int typeAwarePkColumnCount = await GetTypeAwarePrimaryKeyColumnCountAsync(tableName).ConfigureAwait(false);
        ChatMetadata groupChat = await storage.GetChatAsync(new ChatId("telegram", "Group", 42)).ConfigureAwait(false);

        using var _ = Assert.Multiple();
        await Assert.That(typeAwarePkColumnCount).IsEqualTo(1);
        await Assert.That(groupChat.Tags).Contains("group");
    }

    private static SqlServerChatStorage CreateStorage(string tableName)
    {
        return new SqlServerChatStorage(
            Container.GetConnectionString(),
            tableName,
            SchemaName
        );
    }

    private static async Task CreateLegacySchemaAsync(string tableName)
    {
        await using SqlConnection connection = new(Container.GetConnectionString());
        await connection.OpenAsync().ConfigureAwait(false);

        string commandText =
            $"""
            IF OBJECT_ID(N'[{SchemaName}].[{tableName}]', N'U') IS NOT NULL
            BEGIN
                DROP TABLE [{SchemaName}].[{tableName}];
            END

            CREATE TABLE [{SchemaName}].[{tableName}] (
                [System] [nvarchar](20) NOT NULL,
                [Id] bigint NOT NULL,
                [Type] [nvarchar](50) NOT NULL,
                [Title] [nvarchar](250) NOT NULL,
                CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED
                (
                    [System] ASC,
                    [Id] ASC
                )
            );
            """;

        await using SqlCommand command = new(commandText, connection);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static async Task InsertLegacyRowAsync(
        string tableName,
        string system,
        long id,
        string type,
        string tagsJson
    )
    {
        await using SqlConnection connection = new(Container.GetConnectionString());
        await connection.OpenAsync().ConfigureAwait(false);

        await using SqlCommand addTagsColumnCommand = new(
            $"""
            ALTER TABLE [{SchemaName}].[{tableName}] ADD [Tags] [nvarchar](max) NULL;
            """,
            connection
        );
        await addTagsColumnCommand.ExecuteNonQueryAsync().ConfigureAwait(false);

        await using SqlCommand insertCommand = new(
            $"""

            INSERT INTO [{SchemaName}].[{tableName}] ([System], [Id], [Type], [Title], [Tags])
            VALUES (@System, @Id, @Type, @Title, @Tags);
            """,
            connection
        );

        insertCommand.Parameters.Add(new SqlParameter("@System", SqlDbType.NVarChar, 20) { Value = system });
        insertCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = id });
        insertCommand.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar, 50) { Value = type });
        insertCommand.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 250) { Value = string.Empty });
        insertCommand.Parameters.Add(new SqlParameter("@Tags", SqlDbType.NVarChar) { Value = tagsJson });

        await insertCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static async Task<int> GetTypeAwarePrimaryKeyColumnCountAsync(string tableName)
    {
        await using SqlConnection connection = new(Container.GetConnectionString());
        await connection.OpenAsync().ConfigureAwait(false);

        await using SqlCommand command = new(
            $"""
            SELECT COUNT(*)
            FROM sys.key_constraints kc
            JOIN sys.index_columns ic
                ON ic.object_id = kc.parent_object_id
                AND ic.index_id = kc.unique_index_id
            JOIN sys.columns c
                ON c.object_id = ic.object_id
                AND c.column_id = ic.column_id
            WHERE kc.parent_object_id = OBJECT_ID(N'[{SchemaName}].[{tableName}]')
                AND kc.[type] = 'PK'
                AND c.name = 'Type';
            """,
            connection
        );

        object result = await command.ExecuteScalarAsync().ConfigureAwait(false)
            ?? throw new InvalidOperationException("PK inspection returned null.");

        return Convert.ToInt32(result);
    }

    private static string UniqueTableName() => $"Chats_{Guid.NewGuid():N}";

    private static MsSqlContainer Container =>
        _container ?? throw new InvalidOperationException("SQL Server container has not been started.");
}
