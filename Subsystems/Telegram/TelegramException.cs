namespace DioRed.Vermilion.Subsystems.Telegram;

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