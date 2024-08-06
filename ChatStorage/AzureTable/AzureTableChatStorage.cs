using System.Text.Json;

using Azure;
using Azure.Data.Tables;

using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;

public class AzureTableChatStorage(
    AzureStorageSettings settings,
    string tableName = "Chats"
) : IChatStorage
{
    private readonly TableClient _tableClient = new AzureStorageClient(settings).Table(tableName);

    public async Task AddChatAsync(ChatInfo chatInfo, string title)
    {
        await _tableClient.CreateIfNotExistsAsync();

        ChatTableEntity entity = new()
        {
            PartitionKey = chatInfo.ChatId.System,
            Type = chatInfo.ChatId.Type,
            RowKey = chatInfo.ChatId.Id.ToString(),
            Title = title,
            Tags = BuildTagsString(chatInfo.Tags)
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<ChatInfo> GetChatAsync(ChatId chatId)
    {
        await _tableClient.CreateIfNotExistsAsync();

        var response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
             partitionKey: chatId.System,
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

    public async Task<ChatInfo[]> GetChatsAsync()
    {
        await _tableClient.CreateIfNotExistsAsync();

        List<ChatInfo> chats = [];

        await foreach (var entity in _tableClient.QueryAsync<ChatTableEntity>())
        {
            chats.Add(BuildEntity(entity));
        }

        return [.. chats];
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        await _tableClient.DeleteEntityAsync(
            chatId.System,
            chatId.Id.ToString()
        );
    }

    public async Task UpdateChatAsync(ChatInfo chatInfo)
    {
        var response = await _tableClient.GetEntityIfExistsAsync<ChatTableEntity>(
            partitionKey: chatInfo.ChatId.System,
            rowKey: chatInfo.ChatId.Id.ToString()
        );

        if (!response.HasValue)
        {
            throw new InvalidOperationException($"Chat {chatInfo.ChatId} not found");
        }

        ChatTableEntity entity = response.Value!;

        entity.Tags = BuildTagsString(chatInfo.Tags);

        await _tableClient.UpdateEntityAsync(entity, ETag.All);
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

    private static ChatInfo BuildEntity(ChatTableEntity entity)
    {
        return new ChatInfo
        {
            ChatId = new ChatId
            {
                System = entity.PartitionKey,
                Id = long.Parse(entity.RowKey),
                Type = entity.Type ?? "",
            },
            Tags = ParseTagsString(entity.Tags)
        };
    }

    private class ChatTableEntity : BaseTableEntity
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Tags { get; set; }
    }
}