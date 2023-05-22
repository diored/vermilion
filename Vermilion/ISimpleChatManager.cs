using Telegram.Bot.Types;

namespace DioRed.Vermilion;

public interface ISimpleChatManager
{
    void AddChat(Chat chat);
    ICollection<long> GetChatIds();
    void RemoveChat(long chatId);
}