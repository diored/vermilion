namespace DioRed.Vermilion.Subsystems.Telegram.L10n;

internal static class LogMessages
{
    public const string MessagePollingError_1 = "{Type} error occurred during message polling";
    public const string ChatBlocked_2 = "Chat {ChatId} was probably blocked. Message: {Message}";
    public const string GroupUpgradedToSuperGroup_2 = "Group Chat #{ChatId} was upgraded to a supergroup chat #{NewChatId}";
    public const string ChatNotFound_1 = "Chat #{ChatId} was not found";
    public const string NotEnoughRights_1 = "Not enough rights to post in the chat #{ChatId}";
    public const string ConnectionClosed_0 = "Connection was forcibly closed by the remote host";
}