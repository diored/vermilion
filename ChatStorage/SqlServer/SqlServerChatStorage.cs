using System.Text.Json;
using System.Text.RegularExpressions;

using Dapper;

using DioRed.Common;

using Microsoft.Data.SqlClient;

namespace DioRed.Vermilion.ChatStorage;

public partial class SqlServerChatStorage(
    string connectionString,
    string table = "Chats",
    string schema = "dbo"
) : IChatStorage
{
    #region Ctor validation

#pragma warning disable IDE0052 // Remove unread private members
    private readonly Unit __validator = Validate(schema, table);
#pragma warning restore IDE0052 // Remove unread private members

    private static Unit Validate(string schema, string table)
    {
        ValidateIdentifier("Schema", schema);
        ValidateIdentifier("Table", table);

        return default;
    }

    private static void ValidateIdentifier(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} couldn't be empty");
        }

        if (!SimpleIdentifierRegex().IsMatch(value))
        {
            throw new InvalidOperationException($"{name} contains unexpected characters");
        }
    }
    #endregion

    public async Task AddChatAsync(ChatInfo chatInfo, string title)
    {
        using SqlConnection db = new(connectionString);

        await EnsureTableExistsAsync(db);

        await db.ExecuteAsync(
            $"""
            INSERT INTO [{schema}].[{table}]
            ([System], [Id], [Type], [Title], [Tags])
            VALUES
            (@System, @Id, @Type, @Title, @Tags)
            """,
            new
            {
                System = chatInfo.ChatId.System,
                Id = chatInfo.ChatId.Id,
                Type = chatInfo.ChatId.Type,
                Title = title,
                Tags = BuildTagsString(chatInfo.Tags)
            }
        );
    }

    public async Task<ChatInfo> GetChatAsync(ChatId chatId)
    {
        using SqlConnection db = new(connectionString);

        await EnsureTableExistsAsync(db);

        ChatInfoDto? dto = await db.QueryFirstAsync<ChatInfoDto>(
            $"""
            SELECT TOP 1 [System], [Id], [Type], [Tags]
            FROM [{schema}].[{table}]
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            new
            {
                System = chatId.System,
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

    public async Task<ChatInfo[]> GetChatsAsync()
    {
        using SqlConnection db = new(connectionString);

        await EnsureTableExistsAsync(db);

        IEnumerable<ChatInfoDto> dtos = await db.QueryAsync<ChatInfoDto>(
            $"""
            SELECT [System], [Id], [Type], [Tags]
            FROM [{schema}].[{table}]
            """
        );

        return [.. dtos.Select(BuildEntity)];
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        using SqlConnection db = new(connectionString);

        await db.ExecuteAsync(
            $"""
            DELETE FROM [{schema}].[{table}]
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            new
            {
                System = chatId.System,
                Id = chatId.Id,
            }
        );
    }

    public async Task UpdateChatAsync(ChatInfo chatInfo)
    {
        using SqlConnection db = new(connectionString);

        await db.ExecuteAsync(
            $"""
            UPDATE [{schema}].[{table}]
            SET [Tags] = @Tags
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            new
            {
                System = chatInfo.ChatId.System,
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
                WHERE TABLE_NAME = '{table}'
                    AND TABLE_SCHEMA = '{schema}'
            )
            BEGIN
                CREATE TABLE [{schema}].[{table}] (
                    [System] [nvarchar](20) NOT NULL,
                    [Id] bigint NOT NULL,
                    [Type] [nvarchar](50) NOT NULL,
                    [Title] [nvarchar](250) NOT NULL,
                  CONSTRAINT [PK_{table}] PRIMARY KEY CLUSTERED
                  (
                    [System] ASC,
                    [Id] ASC
                  ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ) ON [PRIMARY]
            END
            """
        );
    }

    private static string BuildTagsString(string[] tags)
    {
        return JsonSerializer.Serialize(tags);
    }

    private static string[] ParseTagsString(string? tagsString)
    {
        return tagsString is null or ""
            ? []
            : JsonSerializer.Deserialize<string[]>(tagsString) ?? [];
    }

    private static ChatInfo BuildEntity(ChatInfoDto dto)
    {
        return new ChatInfo
        {
            ChatId = new ChatId
            {
                System = dto.System,
                Id = dto.Id,
                Type = dto.Type
            },
            Tags = ParseTagsString(dto.Tags)
        };
    }

    [GeneratedRegex("""(?:\w+\.)?\w+""")]
    private static partial Regex SimpleIdentifierRegex();

    private record ChatInfoDto(string System, long Id, string Type, string? Tags);
}