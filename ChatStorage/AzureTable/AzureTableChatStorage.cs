using System.Text.Json;

using Azure;
using Azure.Data.Tables;

using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;

public class AzureTableChatStorage(
    AzureStorageSettings settings,
    string tableName = Defaults.TableName
) : IChatStorage
{
    private readonly TableClient _tableClient = new AzureStorageClient(settings).Table(tableName);

    public Task AddChatAsync(ChatMetadata metadata)
    {
        return AddChatAsync(metadata, string.Empty);
    }

    public async Task AddChatAsync(ChatMetadata metadata, string title)
    {
        await _tableClient.CreateIfNotExistsAsync();

        ChatTableEntity entity = new()
        {
            PartitionKey = metadata.ChatId.ConnectorKey,
            Type = metadata.ChatId.Type,
            RowKey = metadata.ChatId.Id.ToString(),
            Title = title,
            Tags = BuildTagsString(metadata.Tags)
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<ChatMetadata> GetChatAsync(ChatId chatId)
    {
        await _tableClient.CreateIfNotExistsAsync();

        var response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
             partitionKey: chatId.ConnectorKey,
             rowKey: chatId.Id.ToString()
         );

        if (!response.HasValue)
        {
            throw new ArgumentException(
                message: $"Chat {chatId} not found",
                paramName: nameof(chatId)
            );
        }

        return BuildEntity(response.Value!);
    }

    public async Task<ChatMetadata[]> GetChatsAsync()
    {
        await _tableClient.CreateIfNotExistsAsync();

        List<ChatMetadata> chats = [];

        await foreach (var entity in _tableClient.QueryAsync<ChatTableEntity>())
        {
            chats.Add(BuildEntity(entity));
        }

        return [.. chats];
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        await _tableClient.DeleteEntityAsync(
            chatId.ConnectorKey,
            chatId.Id.ToString()
        );
    }

    public async Task UpdateChatAsync(ChatMetadata metadata)
    {
        var response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
            partitionKey: metadata.ChatId.ConnectorKey,
            rowKey: metadata.ChatId.Id.ToString()
        );

        if (!response.HasValue)
        {
            throw new InvalidOperationException($"Chat {metadata.ChatId} not found");
        }

        ChatTableEntity entity = response.Value!;

        entity.Tags = BuildTagsString(metadata.Tags);

        await _tableClient.UpdateEntityAsync(entity, ETag.All);
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
        return new ChatMetadata
        {
            ChatId = new ChatId
            {
                ConnectorKey = entity.PartitionKey,
                Id = long.Parse(entity.RowKey),
                Type = entity.Type ?? "",
            },
            Tags = [.. ParseTagsFromString(entity.Tags)]
        };
    }

    private class ChatTableEntity : BaseTableEntity
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Tags { get; set; }
    }
}