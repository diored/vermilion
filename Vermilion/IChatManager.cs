namespace DioRed.Vermilion;

public interface IChatManager
{
    ChatClient AddAndStoreChatClient(ChatId chatId, ChatClient chatClient, string title);
    ChatClient AddChatClient(ChatId chatId, ChatClient chatClient);
    ICollection<ChatClient> GetAllClients();
    ICollection<ChatId> GetStoredChats(BotSystem system);
    ChatClient? GetClient(ChatId chatId);
    void Remove(ChatId chatId);
}