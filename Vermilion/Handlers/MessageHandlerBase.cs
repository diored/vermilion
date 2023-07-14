using System.Text;

namespace DioRed.Vermilion.Handlers;

public abstract class MessageHandlerBase : IMessageHandler
{
    protected MessageHandlerBase(MessageContext messageContext)
    {
        MessageContext = messageContext;
        ChatWriter = messageContext.Bot.Manager.GetChatWriter(messageContext.ChatId);
    }

    protected MessageContext MessageContext { get; }
    protected IChatWriter ChatWriter { get; }

    public async Task HandleAsync(string message)
    {
        try
        {
            await HandleMessageAsync(message);
        }
        catch (BotBlockedException)
        {
            MessageContext.Bot.Manager.Chats.Remove(MessageContext.ChatId);
        }
        catch (Exception ex)
        {
            await OnExceptionAsync(ex);
        }
    }

    protected virtual async Task OnExceptionAsync(Exception ex)
    {
        StringBuilder errorMessage = new();
        errorMessage.Append("Error occurred");

#if DEBUG
        errorMessage.Append(": ").Append(ex);
#endif

        try
        {
            await ChatWriter.SendTextAsync(errorMessage.ToString());
        }
        catch
        {
        }
    }

    protected abstract Task HandleMessageAsync(string message);
}