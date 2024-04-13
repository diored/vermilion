using Azure.Data.Tables;

using DioRed.Common.AzureStorage;

namespace DioRed.Vermilion.ChatStorage;

public class AzureTableChatStorage(
    AzureStorageSettings settings,
    string tableName = "Chats"
) : IChatStorage
{
    private readonly TableClient _tableClient = new AzureStorageClient(settings).Table(tableName);

    public async Task AddChatAsync(ChatId chatId, string title)
    {
        await _tableClient.CreateIfNotExistsAsync();

        ChatTableEntity entity = new()
        {
            PartitionKey = chatId.System,
            Type = chatId.Type,
            RowKey = chatId.Id.ToString(),
            Title = title
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<ChatId[]> GetChatsAsync()
    {
        await _tableClient.CreateIfNotExistsAsync();

        List<ChatId> chats = [];

        await foreach (var entity in _tableClient.QueryAsync<ChatTableEntity>())
        {
            if (!long.TryParse(entity.RowKey, out long id))
            {
                continue;
            }

            chats.Add(
                new ChatId
                {
                    System = entity.PartitionKey,
                    Id = id,
                    Type = entity.Type ?? ""
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
    }
}