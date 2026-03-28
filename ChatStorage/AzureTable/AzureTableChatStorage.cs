using System.Text.Json;
using System.Globalization;
using System.Runtime.CompilerServices;

using Azure;
using Azure.Data.Tables;

using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;

public class AzureTableChatStorage : IChatStorage
{
    private const int CurrentSchemaVersion = 2;
    private const string SchemaPartitionKey = "__vermilion__";
    private const string SchemaRowKey = "__schema__";

    private readonly TableClient _tableClient;
    private readonly Lazy<Task> _ensureSchema;

    public AzureTableChatStorage(
        AzureStorageSettings settings,
        string tableName = Defaults.TableName
    )
    {
        _tableClient = new AzureStorageClient(settings).Table(tableName);

        _ensureSchema = new(EnsureSchemaUpToDateCoreAsync);
    }

    private Task EnsureSchemaUpToDateAsync() => _ensureSchema.Value;

    public async Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    )
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        ChatTableEntity entity = new()
        {
            PartitionKey = metadata.ChatId.ConnectorKey,
            Type = metadata.ChatId.Type,
            RowKey = BuildRowKey(metadata.ChatId),
            Title = title,
            Tags = BuildTagsString(metadata.Tags)
        };

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        ChatTableEntity? entity = await GetEntityInternalAsync(chatId, ct);

        if (entity is null)
        {
            throw new ChatNotFoundException(chatId);
        }

        return BuildEntity(entity);
    }

    public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        await foreach (var entity in _tableClient.QueryAsync<ChatTableEntity>(cancellationToken: ct))
        {
            if (entity.PartitionKey == SchemaPartitionKey)
            {
                continue;
            }

            yield return BuildEntity(entity);
        }
    }

    public async Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        string rowKey = BuildRowKey(chatId);
        var response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
            chatId.ConnectorKey,
            rowKey,
            cancellationToken: ct
        ).ConfigureAwait(false);

        if (response.HasValue)
        {
            await _tableClient.DeleteEntityAsync(chatId.ConnectorKey, rowKey, cancellationToken: ct).ConfigureAwait(false);
            return;
        }

        string legacyRowKey = BuildLegacyRowKey(chatId);
        response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
            chatId.ConnectorKey,
            legacyRowKey,
            cancellationToken: ct
        ).ConfigureAwait(false);

        if (response.HasValue)
        {
            ChatTableEntity legacyEntity = response.Value!;
            if (string.Equals(legacyEntity.Type, chatId.Type, StringComparison.Ordinal))
            {
                await _tableClient.DeleteEntityAsync(chatId.ConnectorKey, legacyRowKey, cancellationToken: ct).ConfigureAwait(false);
            }
        }
    }

    public async Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        ChatTableEntity? entity = await GetEntityInternalAsync(metadata.ChatId, ct);
        if (entity is null)
        {
            throw new ChatNotFoundException(metadata.ChatId);
        }

        entity.Tags = BuildTagsString(metadata.Tags);
        entity.Type = metadata.ChatId.Type;

        string expectedRowKey = BuildRowKey(metadata.ChatId);
        if (!string.Equals(entity.RowKey, expectedRowKey, StringComparison.Ordinal))
        {
            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, cancellationToken: ct).ConfigureAwait(false);
            entity.RowKey = expectedRowKey;
            await _tableClient.AddEntityAsync(entity, ct).ConfigureAwait(false);
            return;
        }

        await _tableClient.UpdateEntityAsync(entity, ETag.All, cancellationToken: ct).ConfigureAwait(false);
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

    private static ChatMetadata BuildEntity(ChatTableEntity entity)
    {
        if (!TryParseRowKey(entity.RowKey, out string? typeFromKey, out long id))
        {
            typeFromKey = entity.Type ?? "";
            id = long.Parse(entity.RowKey, CultureInfo.InvariantCulture);
        }

        return new ChatMetadata
        {
            ChatId = new ChatId
            {
                ConnectorKey = entity.PartitionKey,
                Id = id,
                Type = typeFromKey,
            },
            Tags = [.. ParseTagsFromString(entity.Tags)]
        };
    }

    private async Task<ChatTableEntity?> GetEntityInternalAsync(ChatId chatId, CancellationToken ct)
    {
        string rowKey = BuildRowKey(chatId);
        var response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
            partitionKey: chatId.ConnectorKey,
            rowKey: rowKey,
            cancellationToken: ct
        ).ConfigureAwait(false);

        if (response.HasValue)
        {
            return response.Value!;
        }

        string legacyRowKey = BuildLegacyRowKey(chatId);
        response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
            partitionKey: chatId.ConnectorKey,
            rowKey: legacyRowKey,
            cancellationToken: ct
        ).ConfigureAwait(false);

        if (!response.HasValue)
        {
            return null;
        }

        ChatTableEntity legacyEntity = response.Value!;
        return string.Equals(legacyEntity.Type, chatId.Type, StringComparison.Ordinal)
            ? legacyEntity
            : null;
    }

    private async Task EnsureSchemaUpToDateCoreAsync()
    {
        await _tableClient.CreateIfNotExistsAsync().ConfigureAwait(false);

        SchemaInfoEntity? schemaInfo = await GetSchemaInfoAsync().ConfigureAwait(false);
        if (schemaInfo?.Version >= CurrentSchemaVersion)
        {
            return;
        }

        await MigrateLegacyKeysAsync().ConfigureAwait(false);
        await SetSchemaInfoAsync(CurrentSchemaVersion).ConfigureAwait(false);
    }

    private async Task<SchemaInfoEntity?> GetSchemaInfoAsync()
    {
        var response = await _tableClient.GetEntityIfExistsAsync<SchemaInfoEntity>(
            SchemaPartitionKey,
            SchemaRowKey
        ).ConfigureAwait(false);

        return response.HasValue ? response.Value! : null;
    }

    private async Task SetSchemaInfoAsync(int version)
    {
        await _tableClient.UpsertEntityAsync(
            new SchemaInfoEntity
            {
                PartitionKey = SchemaPartitionKey,
                RowKey = SchemaRowKey,
                Version = version,
                UpdatedUtc = DateTimeOffset.UtcNow,
                ETag = ETag.All
            }
        ).ConfigureAwait(false);
    }

    private async Task MigrateLegacyKeysAsync()
    {
        await foreach (ChatTableEntity entity in _tableClient.QueryAsync<ChatTableEntity>())
        {
            if (entity.PartitionKey == SchemaPartitionKey)
            {
                continue;
            }

            if (TryParseRowKey(entity.RowKey, out _, out _))
            {
                continue;
            }

            if (!long.TryParse(entity.RowKey, NumberStyles.Integer, CultureInfo.InvariantCulture, out long id))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entity.Type))
            {
                throw new StorageMigrationException(
                    $"Cannot migrate Azure Table chat '{entity.PartitionKey}/{entity.RowKey}' because its Type is empty."
                );
            }

            string targetRowKey = BuildRowKey(new ChatId(entity.PartitionKey, entity.Type, id));
            var targetResponse = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
                entity.PartitionKey,
                targetRowKey
            ).ConfigureAwait(false);

            if (targetResponse.HasValue)
            {
                ChatTableEntity target = targetResponse.Value!;

                if (string.IsNullOrWhiteSpace(target.Title) && !string.IsNullOrWhiteSpace(entity.Title))
                {
                    target.Title = entity.Title;
                }

                if (string.IsNullOrWhiteSpace(target.Tags) && !string.IsNullOrWhiteSpace(entity.Tags))
                {
                    target.Tags = entity.Tags;
                }

                if (string.IsNullOrWhiteSpace(target.Type))
                {
                    target.Type = entity.Type;
                }

                await _tableClient.UpdateEntityAsync(target, ETag.All).ConfigureAwait(false);
            }
            else
            {
                ChatTableEntity migrated = new()
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = targetRowKey,
                    Type = entity.Type,
                    Title = entity.Title,
                    Tags = entity.Tags
                };

                await _tableClient.AddEntityAsync(migrated).ConfigureAwait(false);
            }

            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All).ConfigureAwait(false);
        }
    }

    private static string BuildLegacyRowKey(ChatId chatId)
    {
        return chatId.Id.ToString(CultureInfo.InvariantCulture);
    }

    private static string BuildRowKey(ChatId chatId)
    {
        return $"{Uri.EscapeDataString(chatId.Type)}|{BuildLegacyRowKey(chatId)}";
    }

    private static bool TryParseRowKey(string rowKey, out string type, out long id)
    {
        int separatorIndex = rowKey.IndexOf('|');
        if (separatorIndex <= 0 || separatorIndex == rowKey.Length - 1)
        {
            type = string.Empty;
            id = default;
            return false;
        }

        type = Uri.UnescapeDataString(rowKey[..separatorIndex]);
        return long.TryParse(
            rowKey[(separatorIndex + 1)..],
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out id
        );
    }

    private class ChatTableEntity : BaseTableEntity
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Tags { get; set; }
    }

    private class SchemaInfoEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = SchemaPartitionKey;
        public string RowKey { get; set; } = SchemaRowKey;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public int Version { get; set; }
        public DateTimeOffset UpdatedUtc { get; set; }
    }
}
