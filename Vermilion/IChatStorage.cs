namespace DioRed.Vermilion;

public interface IChatStorage
{
    void AddChat(ChatId chatId, string title);
    ICollection<ChatId> GetChats();
    void RemoveChat(ChatId chatId);
}