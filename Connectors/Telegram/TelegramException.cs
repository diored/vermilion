namespace DioRed.Vermilion.Connectors.Telegram;

internal enum TelegramException
{
    Unexpected,
    BotBlocked,
    GroupUpgraded,
    ChatNotFound,
    TooManyRequests,
    SocketException,
    NotEnoughRights
}