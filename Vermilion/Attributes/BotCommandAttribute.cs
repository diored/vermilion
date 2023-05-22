namespace DioRed.Vermilion.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class BotCommandAttribute : Attribute
{
    public BotCommandAttribute(string command, BotCommandOptions options = BotCommandOptions.PlainText)
        : this(UserRole.AnyUser, command, options)
    {
    }

    public BotCommandAttribute(UserRole userRole, string command, BotCommandOptions options = BotCommandOptions.PlainText)
    {
        UserRole = userRole;
        Command = command;
        Options = options;
    }

    public string Command { get; }
    public BotCommandOptions Options { get; }
    public UserRole UserRole { get; }
}
