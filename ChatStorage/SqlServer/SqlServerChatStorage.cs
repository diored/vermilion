using System.Text.Json;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

using Dapper;

using Microsoft.Data.SqlClient;

namespace DioRed.Vermilion.ChatStorage;

public partial class SqlServerChatStorage : IChatStorage
{
    private const string StorageId = "SqlServerChatStorage";
    private const int CurrentSchemaVersion = 2;

    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly string _schema;
    private readonly Lazy<Task> _ensureSchema;

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
        _ensureSchema = new(EnsureSchemaUpToDateCoreAsync);
    }

    public async Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    )
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        await db.ExecuteAsync(new CommandDefinition(
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
                Title = title ?? string.Empty,
                Tags = BuildTagsString(metadata.Tags)
            },
            cancellationToken: ct
        )).ConfigureAwait(false);
    }

    public async Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        ChatInfoDto? dto = await db.QueryFirstOrDefaultAsync<ChatInfoDto>(new CommandDefinition(
            $"""
            SELECT TOP 1 [System], [Id], [Type], [Tags]
            FROM [{_schema}].[{_tableName}]
            WHERE [System] = @System
                AND [Type] = @Type
                AND [Id] = @Id
            """,
            new
            {
                System = chatId.ConnectorKey,
                Type = chatId.Type,
                Id = chatId.Id
            },
            cancellationToken: ct
        )).ConfigureAwait(false);

        if (dto is null)
        {
            throw new ChatNotFoundException(chatId);
        }

        return BuildEntity(dto);
    }

    public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        IEnumerable<ChatInfoDto> dtos = await db.QueryAsync<ChatInfoDto>(new CommandDefinition(
            $"""
            SELECT [System], [Id], [Type], [Tags]
            FROM [{_schema}].[{_tableName}]
            """,
            cancellationToken: ct
        )).ConfigureAwait(false);

        foreach (ChatInfoDto dto in dtos)
        {
            ct.ThrowIfCancellationRequested();
            yield return BuildEntity(dto);
        }
    }

    public async Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        await db.ExecuteAsync(new CommandDefinition(
            $"""
            DELETE FROM [{_schema}].[{_tableName}]
            WHERE [System] = @System
                AND [Type] = @Type
                AND [Id] = @Id
            """,
            new
            {
                System = chatId.ConnectorKey,
                Type = chatId.Type,
                Id = chatId.Id,
            },
            cancellationToken: ct
        )).ConfigureAwait(false);
    }

    public async Task UpdateChatAsync(ChatMetadata chatInfo, CancellationToken ct = default)
    {
        await using SqlConnection db = new(_connectionString);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        int affected = await db.ExecuteAsync(new CommandDefinition(
            $"""
            UPDATE [{_schema}].[{_tableName}]
            SET [Tags] = @Tags
            WHERE [System] = @System
                AND [Type] = @Type
                AND [Id] = @Id
            """,
            new
            {
                System = chatInfo.ChatId.ConnectorKey,
                Type = chatInfo.ChatId.Type,
                Id = chatInfo.ChatId.Id,
                Tags = BuildTagsString(chatInfo.Tags)
            },
            cancellationToken: ct
        )).ConfigureAwait(false);

        if (affected == 0)
        {
            throw new ChatNotFoundException(chatInfo.ChatId);
        }
    }

    private Task EnsureSchemaUpToDateAsync()
    {
        return _ensureSchema.Value;
    }

    private async Task EnsureSchemaUpToDateCoreAsync()
    {
        await using SqlConnection connection = new(_connectionString);

        await EnsureSchemaVersionTableAsync(connection).ConfigureAwait(false);

        int? storedVersion = await GetStoredSchemaVersionAsync(connection).ConfigureAwait(false);
        if (storedVersion >= CurrentSchemaVersion)
        {
            return;
        }

        await EnsureChatsTableAsync(connection).ConfigureAwait(false);
        await EnsureTagsColumnAsync(connection).ConfigureAwait(false);
        await EnsureTypeAwarePrimaryKeyAsync(connection).ConfigureAwait(false);
        await SetStoredSchemaVersionAsync(connection, CurrentSchemaVersion).ConfigureAwait(false);
    }

    private async Task EnsureSchemaVersionTableAsync(SqlConnection connection)
    {
        await connection.ExecuteAsync(
            $"""
            IF SCHEMA_ID('{_schema}') IS NULL
            BEGIN
                EXEC(N'CREATE SCHEMA [{_schema}]');
            END

            IF OBJECT_ID(N'[{_schema}].[__VermilionSchemaVersions]', N'U') IS NULL
            BEGIN
                CREATE TABLE [{_schema}].[__VermilionSchemaVersions] (
                    [Storage] [nvarchar](100) NOT NULL,
                    [Target] [nvarchar](256) NOT NULL,
                    [Version] [int] NOT NULL,
                    [UpdatedUtc] [datetime2](7) NOT NULL,
                    CONSTRAINT [PK___VermilionSchemaVersions] PRIMARY KEY CLUSTERED
                    (
                        [Storage] ASC,
                        [Target] ASC
                    )
                );
            END
            """
        ).ConfigureAwait(false);
    }

    private async Task<int?> GetStoredSchemaVersionAsync(SqlConnection connection)
    {
        return await connection.QueryFirstOrDefaultAsync<int?>(
            $"""
            SELECT TOP 1 [Version]
            FROM [{_schema}].[__VermilionSchemaVersions]
            WHERE [Storage] = @Storage
                AND [Target] = @Target
            """,
            new
            {
                Storage = StorageId,
                Target = _tableName
            }
        ).ConfigureAwait(false);
    }

    private async Task SetStoredSchemaVersionAsync(SqlConnection connection, int version)
    {
        await connection.ExecuteAsync(
            $"""
            MERGE [{_schema}].[__VermilionSchemaVersions] AS target
            USING (
                SELECT
                    @Storage AS [Storage],
                    @Target AS [Target],
                    @Version AS [Version],
                    SYSUTCDATETIME() AS [UpdatedUtc]
            ) AS source
            ON target.[Storage] = source.[Storage]
                AND target.[Target] = source.[Target]
            WHEN MATCHED THEN
                UPDATE SET
                    [Version] = source.[Version],
                    [UpdatedUtc] = source.[UpdatedUtc]
            WHEN NOT MATCHED THEN
                INSERT ([Storage], [Target], [Version], [UpdatedUtc])
                VALUES (source.[Storage], source.[Target], source.[Version], source.[UpdatedUtc]);
            """,
            new
            {
                Storage = StorageId,
                Target = _tableName,
                Version = version
            }
        ).ConfigureAwait(false);
    }

    private async Task EnsureChatsTableAsync(SqlConnection connection)
    {
        await connection.ExecuteAsync(
            $"""
            IF OBJECT_ID(N'[{_schema}].[{_tableName}]', N'U') IS NULL
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
                        [Type] ASC,
                        [Id] ASC
                    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ) ON [PRIMARY]
            END
            """
        ).ConfigureAwait(false);
    }

    private async Task EnsureTagsColumnAsync(SqlConnection connection)
    {
        await connection.ExecuteAsync(
            $"""
            IF OBJECT_ID(N'[{_schema}].[{_tableName}]', N'U') IS NOT NULL
            BEGIN
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
        ).ConfigureAwait(false);
    }

    private async Task EnsureTypeAwarePrimaryKeyAsync(SqlConnection connection)
    {
        await connection.ExecuteAsync(
            $"""
            IF OBJECT_ID(N'[{_schema}].[{_tableName}]', N'U') IS NOT NULL
                AND EXISTS (
                    SELECT 1
                    FROM sys.key_constraints kc
                    WHERE kc.parent_object_id = OBJECT_ID(N'[{_schema}].[{_tableName}]')
                        AND kc.[type] = 'PK'
                        AND NOT EXISTS (
                            SELECT 1
                            FROM sys.index_columns ic
                            JOIN sys.columns c
                                ON c.object_id = ic.object_id
                                AND c.column_id = ic.column_id
                            WHERE ic.object_id = kc.parent_object_id
                                AND ic.index_id = kc.unique_index_id
                                AND ic.key_ordinal > 0
                                AND c.name = 'Type'
                        )
                )
            BEGIN
                DECLARE @pkName sysname = (
                    SELECT TOP 1 kc.name
                    FROM sys.key_constraints kc
                    WHERE kc.parent_object_id = OBJECT_ID(N'[{_schema}].[{_tableName}]')
                        AND kc.[type] = 'PK'
                );

                EXEC(N'ALTER TABLE [{_schema}].[{_tableName}] DROP CONSTRAINT [' + @pkName + ']');
                EXEC(N'
                    ALTER TABLE [{_schema}].[{_tableName}] ADD CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED
                    (
                        [System] ASC,
                        [Type] ASC,
                        [Id] ASC
                    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ');
            END
            """
        ).ConfigureAwait(false);
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
