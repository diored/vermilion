using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion;

public interface IChatTagManager
{
    Task AddTag(ChatId chatId, string tag);
    Task RemoveTag(ChatId chatId, string tag);
}

public class ChatTagManager(
    IChatStorage chatStorage,
    Func<ChatId, Task> reloadChatClientFunc
) : IChatTagManager
{
    public async Task AddTag(
        ChatId chatId,
        string tag
    )
    {
        var chat = await chatStorage.GetChatAsync(chatId);

        if (chat.Tags.Contains(tag))
        {
            return;
        }

        chat = new ChatInfo
        {
            ChatId = chatId,
            Tags = [.. chat.Tags, tag]
        };

        await chatStorage.UpdateChatAsync(chat);
        await reloadChatClientFunc(chatId);
    }

    public async Task RemoveTag(
        ChatId chatId,
        string tag
    )
    {
        var chat = await chatStorage.GetChatAsync(chatId);

        if (!chat.Tags.Contains(tag))
        {
            return;
        }

        chat = new ChatInfo
        {
            ChatId = chatId,
            Tags = [.. chat.Tags.Except([tag])]
        };

        await chatStorage.UpdateChatAsync(chat);
        await reloadChatClientFunc(chatId);
    }
}