namespace DioRed.Vermilion.L10n;

internal static class LogMessages
{
    public const string CoreVersionInfo_1 = "DioRED Vermilion Core {Version} loaded.";
    public const string CustomGreeting_1 = "{Greeting}";
    public const string BotCoreAlreadyStopped_0 = "Bot core already stopped";
    public const string SubsystemStarted_1 = "{Subsystem} subsystem started";
    public const string SubsystemNotStarted_1 = "{Subsystem} subsystem not started";
    public const string SubsystemStopped_1 = "{Subsystem} subsystem stopped";
    public const string SubsystemNotStopped_1 = "{Subsystem} subsystem not stopped";
    public const string UnsupportedContent_2 = "Cannot post the content of type {ContentType} because it isn't supported by the target subsystem {Subsystem}";
    public const string AccessDenied_1 = "Access to the chat {ChatId} has been denied. This chat will not be counted as a client anymore";
    public const string MessageDeliveryFailed_1 = "Message delivery was failed in the chat {ChatId}";
    public const string UnexpectedException_1 = "Unexpected exception occurred during message posting to the chat {ChatId}";
    public const string MessageHandled_6 = """Message "{Message}" handled as a command "{Command}" in {System} {Type} chat #{ChatId} (user role: {UserRole})""";
    public const string ErrorOccurred_0 = "Error occurred during message handling";
}