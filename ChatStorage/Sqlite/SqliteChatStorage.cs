using System.Text.Json;

using Microsoft.Data.Sqlite;
using System.Runtime.CompilerServices;

namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Persists chat metadata in a SQLite database.
/// </summary>
public class SqliteChatStorage : IChatStorage
{
    private const string StorageId = "SqliteChatStorage";
    private const int CurrentSchemaVersion = 2;

    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly Lazy<Task> _ensureSchema;

    /// <summary>
    /// Creates a SQLite chat storage instance.
    /// </summary>
    public SqliteChatStorage(
        string connectionString,
        string tableName = Defaults.TableName
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName, nameof(tableName));

        _connectionString = connectionString;
        _tableName = tableName;
        _ensureSchema = new(EnsureSchemaUpToDateCoreAsync);
    }

    /// <inheritdoc />
    public async Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(metadata);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            INSERT INTO [{_tableName}] ([System], [Type], [Id], [Title], [Tags])
            VALUES ($system, $type, $id, $title, $tags);
            """;
        command.Parameters.AddWithValue("$system", metadata.ChatId.ConnectorKey);
        command.Parameters.AddWithValue("$type", metadata.ChatId.Type);
        command.Parameters.AddWithValue("$id", metadata.ChatId.Id);
        command.Parameters.AddWithValue("$title", title ?? string.Empty);
        command.Parameters.AddWithValue("$tags", BuildTagsString(metadata.Tags));

        try
        {
            await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new ChatAlreadyExistsException(metadata.ChatId, ex);
        }
    }

    /// <inheritdoc />
    public async Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT [System], [Type], [Id], [Tags]
            FROM [{_tableName}]
            WHERE [System] = $system
                AND [Type] = $type
                AND [Id] = $id
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$system", chatId.ConnectorKey);
        command.Parameters.AddWithValue("$type", chatId.Type);
        command.Parameters.AddWithValue("$id", chatId.Id);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            throw new ChatNotFoundException(chatId);
        }

        return BuildEntity(reader);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT [System], [Type], [Id], [Tags]
            FROM [{_tableName}];
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);

        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            yield return BuildEntity(reader);
        }
    }

    /// <inheritdoc />
    public async Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            DELETE FROM [{_tableName}]
            WHERE [System] = $system
                AND [Type] = $type
                AND [Id] = $id;
            """;
        command.Parameters.AddWithValue("$system", chatId.ConnectorKey);
        command.Parameters.AddWithValue("$type", chatId.Type);
        command.Parameters.AddWithValue("$id", chatId.Id);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            UPDATE [{_tableName}]
            SET [Tags] = $tags
            WHERE [System] = $system
                AND [Type] = $type
                AND [Id] = $id;
            """;
        command.Parameters.AddWithValue("$system", metadata.ChatId.ConnectorKey);
        command.Parameters.AddWithValue("$type", metadata.ChatId.Type);
        command.Parameters.AddWithValue("$id", metadata.ChatId.Id);
        command.Parameters.AddWithValue("$tags", BuildTagsString(metadata.Tags));

        int affected = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (affected == 0)
        {
            throw new ChatNotFoundException(metadata.ChatId);
        }
    }

    private Task EnsureSchemaUpToDateAsync()
    {
        return _ensureSchema.Value;
    }

    private async Task EnsureSchemaUpToDateCoreAsync()
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await EnsureSchemaVersionTableAsync(connection).ConfigureAwait(false);

        int? storedVersion = await GetStoredSchemaVersionAsync(connection).ConfigureAwait(false);
        if (storedVersion >= CurrentSchemaVersion)
        {
            return;
        }

        await EnsureChatsTableAsync(connection).ConfigureAwait(false);
        await EnsureCurrentTableShapeAsync(connection).ConfigureAwait(false);
        await SetStoredSchemaVersionAsync(connection, CurrentSchemaVersion).ConfigureAwait(false);
    }

    private async Task EnsureSchemaVersionTableAsync(SqliteConnection connection)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS [__VermilionSchemaVersions] (
                [Storage] TEXT NOT NULL,
                [Target] TEXT NOT NULL,
                [Version] INTEGER NOT NULL,
                [UpdatedUtc] TEXT NOT NULL,
                PRIMARY KEY ([Storage], [Target])
            );
            """;

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private async Task<int?> GetStoredSchemaVersionAsync(SqliteConnection connection)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT [Version]
            FROM [__VermilionSchemaVersions]
            WHERE [Storage] = $storage
                AND [Target] = $target
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$storage", StorageId);
        command.Parameters.AddWithValue("$target", _tableName);

        object? result = await command.ExecuteScalarAsync().ConfigureAwait(false);
        return result is null or DBNull
            ? null
            : Convert.ToInt32(result);
    }

    private async Task SetStoredSchemaVersionAsync(SqliteConnection connection, int version)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO [__VermilionSchemaVersions] ([Storage], [Target], [Version], [UpdatedUtc])
            VALUES ($storage, $target, $version, $updatedUtc)
            ON CONFLICT([Storage], [Target]) DO UPDATE SET
                [Version] = excluded.[Version],
                [UpdatedUtc] = excluded.[UpdatedUtc];
            """;
        command.Parameters.AddWithValue("$storage", StorageId);
        command.Parameters.AddWithValue("$target", _tableName);
        command.Parameters.AddWithValue("$version", version);
        command.Parameters.AddWithValue("$updatedUtc", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private async Task EnsureChatsTableAsync(SqliteConnection connection)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE IF NOT EXISTS [{_tableName}] (
                [System] TEXT NOT NULL,
                [Type] TEXT NOT NULL,
                [Id] INTEGER NOT NULL,
                [Title] TEXT NOT NULL,
                [Tags] TEXT NULL,
                PRIMARY KEY ([System], [Type], [Id])
            );
            """;

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private async Task EnsureCurrentTableShapeAsync(SqliteConnection connection)
    {
        List<ColumnInfo> columns = await GetColumnsAsync(connection).ConfigureAwait(false);
        bool hasTags = columns.Any(x => string.Equals(x.Name, "Tags", StringComparison.OrdinalIgnoreCase));
        bool typeIsPartOfPrimaryKey = columns.Any(x =>
            string.Equals(x.Name, "Type", StringComparison.OrdinalIgnoreCase) &&
            x.PrimaryKeyOrdinal > 0
        );

        if (hasTags && typeIsPartOfPrimaryKey)
        {
            return;
        }

        string tempTableName = $"{_tableName}__migration";

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync().ConfigureAwait(false);
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            $"""
            DROP TABLE IF EXISTS [{tempTableName}];

            CREATE TABLE [{tempTableName}] (
                [System] TEXT NOT NULL,
                [Type] TEXT NOT NULL,
                [Id] INTEGER NOT NULL,
                [Title] TEXT NOT NULL,
                [Tags] TEXT NULL,
                PRIMARY KEY ([System], [Type], [Id])
            );

            INSERT INTO [{tempTableName}] ([System], [Type], [Id], [Title], [Tags])
            SELECT
                [System],
                [Type],
                [Id],
                [Title],
                {GetTagsProjection(columns)}
            FROM [{_tableName}];

            DROP TABLE [{_tableName}];
            ALTER TABLE [{tempTableName}] RENAME TO [{_tableName}];
            """;

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        await transaction.CommitAsync().ConfigureAwait(false);
    }

    private async Task<List<ColumnInfo>> GetColumnsAsync(SqliteConnection connection)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"""PRAGMA table_info([{_tableName}]);""";

        await using SqliteDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        List<ColumnInfo> columns = [];

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            columns.Add(new ColumnInfo(
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetInt32(reader.GetOrdinal("pk"))
            ));
        }

        return columns;
    }

    private static string GetTagsProjection(IEnumerable<ColumnInfo> columns)
    {
        return columns.Any(x => string.Equals(x.Name, "Tags", StringComparison.OrdinalIgnoreCase))
            ? "COALESCE([Tags], '[]')"
            : "'[]'";
    }

    private static string BuildTagsString(IEnumerable<string> tags)
    {
        return JsonSerializer.Serialize(tags.ToArray());
    }

    private static string[] ParseTagsFromString(string? tagsString)
    {
        return string.IsNullOrWhiteSpace(tagsString)
            ? []
            : JsonSerializer.Deserialize<string[]>(tagsString) ?? [];
    }

    private static ChatMetadata BuildEntity(SqliteDataReader reader)
    {
        return new ChatMetadata
        {
            ChatId = new ChatId(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt64(2)
            ),
            Tags = [.. ParseTagsFromString(reader.IsDBNull(3) ? null : reader.GetString(3))]
        };
    }

    private readonly record struct ColumnInfo(string Name, int PrimaryKeyOrdinal);
}
