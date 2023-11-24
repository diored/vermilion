using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DioRed.Vermilion.Telegram;

public class TelegramChatWriter(ITelegramBotClient botClient, long chatId) : IChatWriter
{
    public async Task SendTextAsync(string text)
    {
        await Execute(() => botClient.SendTextMessageAsync(chatId, text));
    }

    public async Task SendTextAsync(string text, IReplyMarkup replyMarkup)
    {
        await Execute(() => botClient.SendTextMessageAsync(chatId, text, replyMarkup: replyMarkup));
    }

    public async Task SendHtmlAsync(string html)
    {
        await Execute(() => botClient.SendTextMessageAsync(chatId, html, parseMode: ParseMode.Html));
    }

    public async Task SendPhotoAsync(string url)
    {
        InputFile photo = InputFile.FromUri(url);

        await Execute(() => botClient.SendPhotoAsync(chatId, photo));
    }

    public async Task SendPhotoAsync(Stream stream)
    {
        InputFile photo = InputFile.FromStream(stream);

        await Execute(() => botClient.SendPhotoAsync(chatId, photo));
    }

    private static async Task Execute(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex) when (ex.Message.Contains("blocked") || ex.Message.Contains("kicked"))
        {
            throw new BotBlockedException(ex);
        }
    }
}