using System.Text.Json;

using DioRed.Vermilion.ChatStorage.JsonFile.L10n;

namespace DioRed.Vermilion.ChatStorage;

public sealed class JsonFileChatStorage : IChatStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _gate = new(1, 1);

    // Cache loaded on first access. Protected by _gate.
    private bool _loaded;
    private readonly Dictionary<ChatId, StoredChat> _chats = new();

    public JsonFileChatStorage(string filePath, bool writeIndented = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = writeIndented
        };
    }

    public Task AddChatAsync(ChatMetadata metadata) => AddChatAsync(metadata, string.Empty);

    public async Task AddChatAsync(ChatMetadata metadata, string title)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);

            if (_chats.ContainsKey(metadata.ChatId))
            {
                throw new InvalidOperationException(ExceptionMessages.ChatAlreadyStored_0);
            }

            _chats[metadata.ChatId] = new StoredChat
            {
                ChatId = metadata.ChatId,
                Title = title ?? string.Empty,
                Tags = [.. metadata.Tags]
            };

            await PersistAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<ChatMetadata> GetChatAsync(ChatId chatId)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);

            if (!_chats.TryGetValue(chatId, out StoredChat? stored))
            {
                throw new ArgumentException($"Chat {chatId} not found", nameof(chatId));
            }

            return stored.ToMetadata();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<ChatMetadata[]> GetChatsAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            return _chats.Values.Select(x => x.ToMetadata()).ToArray();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);

            if (_chats.Remove(chatId))
            {
                await PersistAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpdateChatAsync(ChatMetadata metadata)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await EnsureLoadedAsync().ConfigureAwait(false);

            if (!_chats.TryGetValue(metadata.ChatId, out StoredChat? existing))
            {
                throw new ArgumentException($"Chat {metadata.ChatId} not found", nameof(metadata));
            }

            existing.Tags = [.. metadata.Tags];
            _chats[metadata.ChatId] = existing;

            await PersistAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        _loaded = true;

        if (!File.Exists(_filePath))
        {
            return;
        }

        string json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return;

        StorageFile? file = JsonSerializer.Deserialize<StorageFile>(json, _jsonOptions);
        if (file?.Chats is null) return;

        _chats.Clear();
        foreach (StoredChat chat in file.Chats)
        {
            _chats[chat.ChatId] = chat;
        }
    }

    private async Task PersistAsync()
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
        await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);

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
    }
}
