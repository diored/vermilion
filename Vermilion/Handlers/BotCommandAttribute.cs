namespace DioRed.Vermilion.Handlers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class BotCommandAttribute(UserRole userRole, string command, BotCommandOptions options = BotCommandOptions.PlainText) : Attribute
{
    public BotCommandAttribute(string command, BotCommandOptions options = BotCommandOptions.PlainText)
        : this(UserRole.Member, command, options)
    {
    }

    public string Command { get; } = command;
    public BotCommandOptions Options { get; } = options;
    public UserRole UserRole { get; } = userRole;
}
