using System.Text.Json;
using System.Text.RegularExpressions;

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
        using SqlConnection connection = new(connectionString);

        using SqlCommand command = new(
            $"""
            INSERT INTO [{schema}].[{table}]
            ([System], [Id], [Type], [Title], [Tags])
            VALUES
            (@System, @Id, @Type, @Title, @Tags)
            """,
            connection
        );

        command.Parameters.AddWithValue("@System", chatInfo.ChatId.System);
        command.Parameters.AddWithValue("@Id", chatInfo.ChatId.Id);
        command.Parameters.AddWithValue("@Type", chatInfo.ChatId.Type);
        command.Parameters.AddWithValue("@Title", title);
        command.Parameters.AddWithValue("@Tags", JsonSerializer.Serialize(chatInfo.Tags));

        await connection.OpenAsync();

        await EnsureTableExistsAsync(connection);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<ChatInfo[]> GetChatsAsync()
    {
        using SqlConnection connection = new(connectionString);

        using SqlCommand command = new(
            $"""
            SELECT [System], [Id], [Type], [Tags]
            FROM [{schema}].[{table}]
            """,
            connection
        );

        await connection.OpenAsync();

        await EnsureTableExistsAsync(connection);

        List<ChatInfo> chats = [];

        SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            chats.Add(
                new ChatInfo
                {
                    ChatId = new ChatId
                    {
                        System = reader.GetString(0),
                        Id = reader.GetInt64(1),
                        Type = reader.GetString(2)
                    },
                    Tags = reader.IsDBNull(3)
                        ? []
                        : JsonSerializer.Deserialize<string[]>(reader.GetString(3)) ?? []
                }
            );
        }

        return [.. chats];
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        using SqlConnection connection = new(connectionString);

        using SqlCommand command = new(
            $"""
            DELETE FROM [{schema}].[{table}]
            WHERE [System] = @System
                AND [Id] = @Id
            """,
            connection
        );

        command.Parameters.AddWithValue("@System", chatId.System);
        command.Parameters.AddWithValue("@Id", chatId.Id);

        await connection.OpenAsync();

        await EnsureTableExistsAsync(connection);

        await command.ExecuteNonQueryAsync();
    }

    private async Task EnsureTableExistsAsync(SqlConnection connection)
    {
        using SqlCommand command = new(
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
            """,
            connection
        );

        await command.ExecuteNonQueryAsync();
    }

    [GeneratedRegex("""(?:\w+\.)?\w+""")]
    private static partial Regex SimpleIdentifierRegex();
}