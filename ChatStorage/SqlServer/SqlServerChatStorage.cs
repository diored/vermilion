using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;

namespace DioRed.Vermilion.ChatStorage;

public partial class SqlServerChatStorage(
    string connectionString,
    string table = "Chats",
    string schema = "dbo"
) : IChatStorage
{
    private readonly string _table = ValidateIdentifier("Table", table);
    private readonly string _schema = ValidateIdentifier("Schema", schema);

    public async Task AddChatAsync(ChatId chatId, string title)
    {
        using SqlConnection connection = new(connectionString);

        using SqlCommand command = new(
            $"""
            INSERT INTO [{_table}]
            ([System], [Id], [Type], [Title])
            VALUES
            (@System, @Id, @Type, @Title)
            """,
            connection
        );

        command.Parameters.AddWithValue("@System", chatId.System);
        command.Parameters.AddWithValue("@Id", chatId.Id);
        command.Parameters.AddWithValue("@Type", chatId.Type);
        command.Parameters.AddWithValue("@Title", title);

        await connection.OpenAsync();

        await EnsureTableExistsAsync(connection);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<ChatId[]> GetChatsAsync()
    {
        using SqlConnection connection = new(connectionString);

        using SqlCommand command = new(
            $"""
            SELECT [System], [Id], [Type]
            FROM [{_schema}].[{_table}]
            """,
            connection
        );

        await connection.OpenAsync();

        await EnsureTableExistsAsync(connection);

        List<ChatId> chats = [];

        SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            chats.Add(
                new ChatId
                {
                    System = reader.GetString(0),
                    Id = reader.GetInt64(1),
                    Type = reader.GetString(2)
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
            DELETE FROM [{_schema}].[{_table}]
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
                WHERE TABLE_NAME = '{_table}'
                    AND TABLE_SCHEMA = '{_schema}'
            )
            BEGIN
                CREATE TABLE {_schema}.{_table} (
                    [System] [nvarchar](20) NOT NULL,
                    [Id] bigint NOT NULL,
                    [Type] [nvarchar](50) NOT NULL,
                    [Title] [nvarchar](250) NOT NULL,
                  CONSTRAINT [PK_{_table}] PRIMARY KEY CLUSTERED
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

    private static string ValidateIdentifier(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} couldn't be empty");
        }

        if (!SimpleIdentifierRegex().IsMatch(value))
        {
            throw new InvalidOperationException($"{name} contains unexpected characters");
        }

        return value;
    }

    [GeneratedRegex("""(?:\w+\.)?\w+""")]
    private static partial Regex SimpleIdentifierRegex();
}