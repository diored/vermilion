using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DioRed.Vermilion.Telegram;

public class TelegramChatWriter : IChatWriter
{
    private readonly ITelegramBotClient _botClient;
    private readonly long _chatId;

    public TelegramChatWriter(ITelegramBotClient botClient, long chatId)
    {
        _botClient = botClient;
        _chatId = chatId;
    }

    public async Task SendTextAsync(string text)
    {
        await Execute(() => _botClient.SendTextMessageAsync(_chatId, text));
    }

    public async Task SendTextAsync(string text, IReplyMarkup replyMarkup)
    {
        await Execute(() => _botClient.SendTextMessageAsync(_chatId, text, replyMarkup: replyMarkup));
    }

    public async Task SendHtmlAsync(string html)
    {
        await Execute(() => _botClient.SendTextMessageAsync(_chatId, html, parseMode: ParseMode.Html));
    }

    public async Task SendPhotoAsync(string url)
    {
        InputFile photo = InputFile.FromUri(url);

        await Execute(() => _botClient.SendPhotoAsync(_chatId, photo));
    }

    public async Task SendPhotoAsync(Stream stream)
    {
        InputFile photo = InputFile.FromStream(stream);

        await Execute(() => _botClient.SendPhotoAsync(_chatId, photo));
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