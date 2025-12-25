using System.Text.Json;
using System.Text.RegularExpressions;

using Dapper;

using Microsoft.Data.SqlClient;

namespace DioRed.Vermilion.ChatStorage;

public partial class SqlServerChatStorage : IChatStorage
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly string _schema;

    public SqlServerChatStorage(
        string connectionString,
        string tableName = Defaults.TableName,
        string schema = Defaults.Schema
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));
        ArgumentException.ThrowIfNullOrWhiteSpace(schema, nameof(schema));

        if (!SimpleIdentifierRegex().IsMatch(schema))
        {
            throw new ArgumentException(
                "Schema name contains unexpected characters",
                nameof(schema)
            );
        }

        if (!SimpleIdentifierRegex().IsMatch(tableName))
        {
            throw new ArgumentException(
                "Table name contains unexpected characters",
                nameof(tableName)
            );
        }

        _connectionString = connectionString;
        _tableName = tableName;
        _schema = schema;
    }

    public Task AddChatAsync(ChatMetadata metadata)
    {
        return AddChatAsync(metadata, string.Empty);
    }

    public async Task AddChatAsync(ChatMetadata metadata, string title)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureTableExistsAsync(db);

        await db.ExecuteAsync(
            $"""
            INSERT INTO [{_schema}].[{_tableName}]
            ([System], [Id], [Type], [Title], [Tags])
            VALUES
            (@System, @Id, @Type, @Title, @Tags)
            """,
            new
            {
                System = metadata.ChatId.ConnectorKey,
                Id = metadata.ChatId.Id,
                Type = metadata.ChatId.Type,
                Title = title,
                Tags = BuildTagsString(metadata.Tags)
            }
        );
    }

    public async Task<ChatMetadata> GetChatAsync(ChatId chatId)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureTableExistsAsync(db);

        ChatInfoDto? dto = await db.QueryFirstOrDefaultAsync<ChatInfoDto>(
            $"""
            SELECT TOP 1 [System], [Id], [Type], [Tags]
            FROM [{_schema}].[{_tableName}]
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            new
            {
                System = chatId.ConnectorKey,
                Id = chatId.Id
            }
        );

        if (dto is null)
        {
            throw new ArgumentException(
                message: $"Chat {chatId} not found",
                paramName: nameof(chatId)
            );
        }

        return BuildEntity(dto);
    }

    public async Task<ChatMetadata[]> GetChatsAsync()
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureTableExistsAsync(db);

        IEnumerable<ChatInfoDto> dtos = await db.QueryAsync<ChatInfoDto>(
            $"""
            SELECT [System], [Id], [Type], [Tags]
            FROM [{_schema}].[{_tableName}]
            """
        );

        return [.. dtos.Select(BuildEntity)];
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureTableExistsAsync(db);

        await db.ExecuteAsync(
            $"""
            DELETE FROM [{_schema}].[{_tableName}]
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            new
            {
                System = chatId.ConnectorKey,
                Id = chatId.Id,
            }
        );
    }

    public async Task UpdateChatAsync(ChatMetadata chatInfo)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureTableExistsAsync(db);

        await db.ExecuteAsync(
            $"""
            UPDATE [{_schema}].[{_tableName}]
            SET [Tags] = @Tags
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            new
            {
                System = chatInfo.ChatId.ConnectorKey,
                Id = chatInfo.ChatId.Id,
                Tags = BuildTagsString(chatInfo.Tags)
            }
        );
    }

    private async Task EnsureTableExistsAsync(SqlConnection connection)
    {
        await connection.ExecuteAsync(
            $"""
            IF NOT EXISTS (
                SELECT 'X'
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = '{_tableName}'
                    AND TABLE_SCHEMA = '{_schema}'
            )
            BEGIN
                CREATE TABLE [{_schema}].[{_tableName}] (
                    [System] [nvarchar](20) NOT NULL,
                    [Id] bigint NOT NULL,
                    [Type] [nvarchar](50) NOT NULL,
                    [Title] [nvarchar](250) NOT NULL,
                    [Tags] [nvarchar](max) NULL,
                  CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED
                  (
                    [System] ASC,
                    [Id] ASC
                  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ) ON [PRIMARY]
            END
            ELSE
            BEGIN
                -- Backward-compatible migration: older versions created the table without the [Tags] column.
                IF NOT EXISTS (
                    SELECT 'X'
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = '{_tableName}'
                        AND TABLE_SCHEMA = '{_schema}'
                        AND COLUMN_NAME = 'Tags'
                )
                BEGIN
                    ALTER TABLE [{_schema}].[{_tableName}] ADD [Tags] [nvarchar](max) NULL;
                END
            END
            """
        );
    }

    private static string BuildTagsString(IEnumerable<string> tags)
    {
        return JsonSerializer.Serialize(tags.ToArray());
    }

    private static string[] ParseTagsFromString(string? tagsString)
    {
        return tagsString is null or ""
            ? []
            : JsonSerializer.Deserialize<string[]>(tagsString) ?? [];
    }

    private static ChatMetadata BuildEntity(ChatInfoDto dto)
    {
        return new ChatMetadata
        {
            ChatId = new ChatId
            {
                ConnectorKey = dto.System,
                Id = dto.Id,
                Type = dto.Type
            },
            Tags = [.. ParseTagsFromString(dto.Tags)]
        };
    }

    [GeneratedRegex("""(?:\w+\.)?\w+""")]
    private static partial Regex SimpleIdentifierRegex();

    private record ChatInfoDto(string System, long Id, string Type, string? Tags);
}