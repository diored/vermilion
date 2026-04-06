using System.Text.Json;

namespace DioRed.Vermilion.ChatStorage;

/// <summary>
/// Persists chat metadata to a JSON file on disk.
/// </summary>
public sealed class JsonFileChatStorage : IChatStorage, IChatStorageExport
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _gate = new(1, 1);

    // Cache loaded on first access. Protected by _gate.
    private bool _loaded;
    private readonly Dictionary<ChatId, StoredChat> _chats = new();

    /// <summary>
    /// Creates a JSON file chat storage instance.
    /// </summary>
    public JsonFileChatStorage(string filePath, bool writeIndented = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = writeIndented
        };
    }

    /// <inheritdoc />
    public async Task AddChatAsync(
        ChatMetadata metadata,
        string? title = null,
        CancellationToken ct = default
    )
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync(ct).ConfigureAwait(false);

            if (_chats.ContainsKey(metadata.ChatId))
            {
                throw new ChatAlreadyExistsException(metadata.ChatId);
            }

            _chats[metadata.ChatId] = new StoredChat
            {
                ChatId = metadata.ChatId,
                Title = title ?? string.Empty,
                Tags = [.. metadata.Tags]
            };

            await PersistAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ChatMetadata> GetChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync(ct).ConfigureAwait(false);

            if (!_chats.TryGetValue(chatId, out StoredChat? stored))
            {
                throw new ChatNotFoundException(chatId);
            }

            return stored.ToMetadata();
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatMetadata> GetChatsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync(ct).ConfigureAwait(false);
            foreach (ChatMetadata chat in _chats.Values.Select(x => x.ToMetadata()).ToArray())
            {
                ct.ThrowIfCancellationRequested();
                yield return chat;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StoredChatRecord> ExportChatsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync(ct).ConfigureAwait(false);
            foreach (StoredChat chat in _chats.Values.ToArray())
            {
                ct.ThrowIfCancellationRequested();
                yield return chat.ToStoredChatRecord();
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveChatAsync(ChatId chatId, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync(ct).ConfigureAwait(false);

            if (_chats.Remove(chatId))
            {
                await PersistAsync(ct).ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task UpdateChatAsync(ChatMetadata metadata, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync(ct).ConfigureAwait(false);

            if (!_chats.TryGetValue(metadata.ChatId, out StoredChat? existing))
            {
                throw new ChatNotFoundException(metadata.ChatId);
            }

            existing.Tags = [.. metadata.Tags];
            _chats[metadata.ChatId] = existing;

            await PersistAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_loaded) return;

        _loaded = true;

        if (!File.Exists(_filePath))
        {
            return;
        }

        string json = await File.ReadAllTextAsync(_filePath, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return;

        StorageFile? file = JsonSerializer.Deserialize<StorageFile>(json, _jsonOptions);
        if (file?.Chats is null) return;

        _chats.Clear();
        foreach (StoredChat chat in file.Chats)
        {
            _chats[chat.ChatId] = chat;
        }
    }

    private async Task PersistAsync(CancellationToken ct)
    {
        string? dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        StorageFile file = new()
        {
            Version = 1,
            Chats = _chats.Values.OrderBy(c => c.ChatId.ConnectorKey)
                .ThenBy(c => c.ChatId.Id)
                .ToList()
        };

        string json = JsonSerializer.Serialize(file, _jsonOptions);

        string tempPath = _filePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, ct).ConfigureAwait(false);

        if (File.Exists(_filePath))
        {
            // Atomic replace where supported.
            File.Replace(tempPath, _filePath, destinationBackupFileName: null);
        }
        else
        {
            File.Move(tempPath, _filePath);
        }
    }

    private sealed class StorageFile
    {
        public int Version { get; set; } = 1;
        public List<StoredChat> Chats { get; set; } = new();
    }

    private sealed class StoredChat
    {
        public required ChatId ChatId { get; init; }
        public string Title { get; set; } = string.Empty;
        public HashSet<string> Tags { get; set; } = [];

        public ChatMetadata ToMetadata()
        {
            return new ChatMetadata
            {
                ChatId = ChatId,
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
}
