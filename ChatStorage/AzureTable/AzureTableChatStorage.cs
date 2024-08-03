using System.Text.Json;

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
            Tags = JsonSerializer.Serialize(chatInfo.Tags)
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<ChatInfo[]> GetChatsAsync()
    {
        await _tableClient.CreateIfNotExistsAsync();

        List<ChatInfo> chats = [];

        await foreach (var entity in _tableClient.QueryAsync<ChatTableEntity>())
        {
            if (!long.TryParse(entity.RowKey, out long id))
            {
                continue;
            }

            chats.Add(
                new ChatInfo
                {
                    ChatId = new ChatId
                    {
                        System = entity.PartitionKey,
                        Id = id,
                        Type = entity.Type ?? "",
                    },
                    Tags = entity.Tags is null or ""
                        ? []
                        : JsonSerializer.Deserialize<string[]>(entity.Tags) ?? []
                }
            );
        }

        return [.. chats];
    }

    public async Task RemoveChatAsync(ChatId chatId)
    {
        await _tableClient.CreateIfNotExistsAsync();

        await _tableClient.DeleteEntityAsync(
            chatId.System,
            chatId.Id.ToString()
        );
    }

    private class ChatTableEntity : BaseTableEntity
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Tags { get; set; }
    }
}