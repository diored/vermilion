namespace DioRed.Vermilion;

public class ChatClient(ChatId chatId, VermilionBot bot)
{
    private readonly Dictionary<string, object> _properties = new();

    public ChatId ChatId { get; } = chatId;

    public object? this[string property]
    {
        get
        {
            _properties.TryGetValue(property, out var value);
            return value;
        }
        set
        {
            if (value is null)
            {
                _properties.Remove(property);
            }
            else
            {
                _properties[property] = value;
            }
        }
    }

    public async Task HandleMessageAsync(string message, long senderId, int messageId, CancellationToken cancellationToken)
    {
        MessageContext messageContext = new()
        {
            Bot = bot,
            ChatId = ChatId,
            MessageId = messageId,
            UserRole = await bot.GetUserRoleAsync(senderId, ChatId, cancellationToken)
        };

        IMessageHandler messageHandler = bot.Manager.GetMessageHandler(messageContext);

        await messageHandler.HandleAsync(message);
    }
}