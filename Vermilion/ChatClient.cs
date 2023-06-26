namespace DioRed.Vermilion;

public class ChatClient
{
    private readonly Dictionary<string, object> _properties = new();
    private readonly VermilionBot _bot;

    public ChatClient(ChatId chatId, VermilionBot bot)
    {
        ChatId = chatId;
        _bot = bot;
    }

    public ChatId ChatId { get; }

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
            Bot = _bot,
            ChatId = ChatId,
            MessageId = messageId,
            UserRole = await _bot.GetUserRoleAsync(senderId, ChatId, cancellationToken)
        };

        IMessageHandler messageHandler = _bot.Manager.GetMessageHandler(messageContext);

        await messageHandler.HandleAsync(message);
    }
}