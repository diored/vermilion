using Telegram.Bot.Types;

namespace DioRed.Vermilion;

public abstract class SimpleBot<TChatClient> : Bot
    where TChatClient : IChatClient
{
    private readonly ISimpleChatManager _chatManager;

    private bool _newChatsDetection;

    public SimpleBot(
        BotConfiguration configuration,
        ISimpleChatManager chatManager,
        CancellationTokenSource cancellationTokenSource)
            : base(configuration, cancellationTokenSource.Token)
    {
        _chatManager = chatManager;

        _newChatsDetection = true;
    }

    protected override void OnChatClientAdded(Chat chat)
    {
        base.OnChatClientAdded(chat);

        if (_newChatsDetection)
        {
            _chatManager.AddChat(chat);
        }
    }

    public override void StartReceiving()
    {
        _newChatsDetection = false;
        ReconnectToChats();
        _newChatsDetection = true;

        base.StartReceiving();
    }

    private void ReconnectToChats()
    {
        ICollection<long> chatIds = _chatManager.GetChatIds();

        foreach (long chatId in chatIds)
        {
            ReconnectToChatAsync(chatId).GetAwaiter().GetResult();
        }
    }

    private async Task ReconnectToChatAsync(long chatId)
    {
        try
        {
            await ConnectToChatAsync(chatId);
        }
        catch (Exception ex) when (ex.Message.Contains("kicked") || ex.Message.Contains("blocked"))
        {
            _chatManager.RemoveChat(chatId);
        }
    }
}