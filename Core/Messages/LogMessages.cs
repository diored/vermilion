namespace DioRed.Vermilion.Messages;

internal static class LogMessages
{
    public const string CoreVersionInfo_1 = "DioRED Vermilion Core {Version} loaded.";
    public const string CustomGreeting_1 = "{Greeting}";
    public const string BotCoreAlreadyStopped_0 = "Bot core already stopped";
    public const string ConnectorStarted_1 = "{Connector} connector started";
    public const string ConnectorStarted_2 = "{Connector} connector {Version} started";
    public const string ConnectorNotStarted_1 = "{Connector} connector not started";
    public const string ConnectorStopped_1 = "{Connector} connector stopped";
    public const string ConnectorNotStopped_1 = "{Connector} connector not stopped";
    public const string UnsupportedContent_2 = "Cannot post the content of type {ContentType} because it isn't supported by the target connector {Connector}";
    public const string AccessDenied_1 = "Access to the chat {ChatId} has been denied. This chat will not be counted as a client anymore";
    public const string MessageDeliveryFailed_1 = "Message delivery was failed in the chat {ChatId}";
    public const string UnexpectedException_1 = "Unexpected exception occurred during message posting to the chat {ChatId}";
    public const string MessageHandled_6 = """Message "{Message}" handled as a command "{Command}" in {ConnectorKey} {Type} chat #{ChatId} (user role: {UserRole})""";
    public const string ErrorOccurred_0 = "Error occurred during message handling";
    public const string ChatRemoved_1 = "Chat {ChatId} removed";
    public const string ChatRemoveFailure_1 = "Chat {ChatId} removal failed";
    public const string ChatAdded_1 = "Chat {ChatId} added";
    public const string ChatAddFailure_1 = "Chat {ChatId} add failure";
}