using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Persists chat metadata in MongoDB.
/// </summary>
public class MongoDbChatStorage : IChatStorage, IChatStorageExport
{
    private const string SchemaCollectionName = "__VermilionSchemaVersions";
    private const string StorageId = "MongoDbChatStorage";
    private const int CurrentSchemaVersion = 2;

    private readonly IMongoCollection<ChatDocument> _collection;
    private readonly IMongoCollection<SchemaVersionDocument> _schemaVersions;
    private readonly Lazy<Task> _ensureSchema;

    /// <summary>
    /// Creates a MongoDB chat storage instance.
    /// </summary>
    public MongoDbChatStorage(
        string connectionString,
        string databaseName = Defaults.DatabaseName,
        string collectionName = Defaults.CollectionName
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName, nameof(databaseName));
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName, nameof(collectionName));

        MongoClient client = new(connectionString);
        IMongoDatabase database = client.GetDatabase(databaseName);

        _collection = database.GetCollection<ChatDocument>(collectionName);
        _schemaVersions = database.GetCollection<SchemaVersionDocument>(SchemaCollectionName);
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

        ChatDocument document = new()
        {
            System = metadata.ChatId.ConnectorKey,
            Type = metadata.ChatId.Type,
            Id = metadata.ChatId.Id,
            Title = title ?? string.Empty,
            Tags = [.. metadata.Tags]
        };

        try
        {
            await _collection.InsertOneAsync(document, cancellationToken: ct).ConfigureAwait(false);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ChatAlreadyExistsException(metadata.ChatId, ex);
        }
    }

    /// <inheritdoc />
    public async Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        ChatDocument? document = await _collection.Find(BuildIdentityFilter(chatId))
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return document is not null
            ? document.ToMetadata()
            : throw new ChatNotFoundException(chatId);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        using IAsyncCursor<ChatDocument> cursor = await _collection.Find(FilterDefinition<ChatDocument>.Empty)
            .ToCursorAsync(ct)
            .ConfigureAwait(false);

        while (await cursor.MoveNextAsync(ct).ConfigureAwait(false))
        {
            foreach (ChatDocument document in cursor.Current)
            {
                ct.ThrowIfCancellationRequested();
                yield return document.ToMetadata();
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StoredChatRecord> ExportChatsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        using IAsyncCursor<ChatDocument> cursor = await _collection.Find(FilterDefinition<ChatDocument>.Empty)
            .ToCursorAsync(ct)
            .ConfigureAwait(false);

        while (await cursor.MoveNextAsync(ct).ConfigureAwait(false))
        {
            foreach (ChatDocument document in cursor.Current)
            {
                ct.ThrowIfCancellationRequested();
                yield return document.ToStoredChatRecord();
            }
        }
    }

    /// <inheritdoc />
    public async Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);
        await _collection.DeleteOneAsync(BuildIdentityFilter(chatId), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        await EnsureSchemaUpToDateAsync().ConfigureAwait(false);

        UpdateResult result = await _collection.UpdateOneAsync(
            BuildIdentityFilter(metadata.ChatId),
            Builders<ChatDocument>.Update.Set(x => x.Tags, [.. metadata.Tags]),
            cancellationToken: ct
        ).ConfigureAwait(false);

        if (result.MatchedCount == 0)
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
        SchemaVersionDocument? currentVersion = await _schemaVersions.Find(
                Builders<SchemaVersionDocument>.Filter.And(
                    Builders<SchemaVersionDocument>.Filter.Eq(x => x.Storage, StorageId),
                    Builders<SchemaVersionDocument>.Filter.Eq(x => x.Target, _collection.CollectionNamespace.CollectionName)
                )
            )
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (currentVersion?.Version >= CurrentSchemaVersion)
        {
            return;
        }

        await EnsureIndexesAsync().ConfigureAwait(false);
        await MigrateDocumentsAsync().ConfigureAwait(false);
        await SetSchemaVersionAsync(CurrentSchemaVersion).ConfigureAwait(false);
    }

    private async Task EnsureIndexesAsync()
    {
        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<ChatDocument>(
                Builders<ChatDocument>.IndexKeys
                    .Ascending(x => x.System)
                    .Ascending(x => x.Type)
                    .Ascending(x => x.Id),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "UX_System_Type_Id"
                }
            )
        ).ConfigureAwait(false);
    }

    private async Task MigrateDocumentsAsync()
    {
        await _collection.UpdateManyAsync(
            Builders<ChatDocument>.Filter.Exists(x => x.Tags, false),
            Builders<ChatDocument>.Update.Set(x => x.Tags, [])
        ).ConfigureAwait(false);

        await _collection.UpdateManyAsync(
            Builders<ChatDocument>.Filter.Exists(x => x.Title, false),
            Builders<ChatDocument>.Update.Set(x => x.Title, string.Empty)
        ).ConfigureAwait(false);
    }

    private async Task SetSchemaVersionAsync(int version)
    {
        await _schemaVersions.ReplaceOneAsync(
            Builders<SchemaVersionDocument>.Filter.And(
                Builders<SchemaVersionDocument>.Filter.Eq(x => x.Storage, StorageId),
                Builders<SchemaVersionDocument>.Filter.Eq(x => x.Target, _collection.CollectionNamespace.CollectionName)
            ),
            new SchemaVersionDocument
            {
                Storage = StorageId,
                Target = _collection.CollectionNamespace.CollectionName,
                Version = version,
                UpdatedUtc = DateTime.UtcNow
            },
            new ReplaceOptions { IsUpsert = true }
        ).ConfigureAwait(false);
    }

    private static FilterDefinition<ChatDocument> BuildIdentityFilter(ChatId chatId)
    {
        return Builders<ChatDocument>.Filter.And(
            Builders<ChatDocument>.Filter.Eq(x => x.System, chatId.ConnectorKey),
            Builders<ChatDocument>.Filter.Eq(x => x.Type, chatId.Type),
            Builders<ChatDocument>.Filter.Eq(x => x.Id, chatId.Id)
        );
    }

    [BsonIgnoreExtraElements]
    private sealed class ChatDocument
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }

        [BsonElement("system")]
        public required string System { get; init; }

        [BsonElement("type")]
        public required string Type { get; init; }

        [BsonElement("id")]
        public required long Id { get; init; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("tags")]
        public string[] Tags { get; set; } = [];

        public ChatMetadata ToMetadata()
        {
            return new ChatMetadata
            {
                ChatId = new ChatId(System, Type, Id),
                Tags = [.. Tags]
            };
        }

        public StoredChatRecord ToStoredChatRecord()
        {
            return new StoredChatRecord
            {
                Metadata = ToMetadata(),
                Title = Title
            };
        }
    }

    [BsonIgnoreExtraElements]
    private sealed class SchemaVersionDocument
    {
        [BsonId]
        public string Id => $"{Storage}:{Target}";

        [BsonElement("storage")]
        public required string Storage { get; init; }

        [BsonElement("target")]
        public required string Target { get; init; }

        [BsonElement("version")]
        public required int Version { get; init; }

        [BsonElement("updatedUtc")]
        public required DateTime UpdatedUtc { get; init; }
    }
}
