using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class BotCommandAttribute : Attribute
{
    public BotCommandAttribute(string command, BotCommandOptions options = BotCommandOptions.EqualsTo)
        : this(UserRole.AnyUser, command, options)
    {
    }

    public BotCommandAttribute(UserRole role, string command, BotCommandOptions options = BotCommandOptions.EqualsTo)
    {
        Role = role;

        RegexOptions regexOptions = RegexOptions.Multiline;

        if (options.HasFlag(BotCommandOptions.CaseInsensitive))
        {
            regexOptions |= RegexOptions.IgnoreCase;
        }

        options &= ~BotCommandOptions.CaseInsensitive;

        string escaped = Regex.Escape(command);

        string pattern = options switch
        {
            BotCommandOptions.EqualsTo => $"^{escaped}$",
            BotCommandOptions.StartsWith => $"^{escaped} (.+)$",
            BotCommandOptions.Regex => command,
            _ => throw new ArgumentException("Unsupported options: " + options)
        };

        Regex = new Regex(pattern, regexOptions);
    }

    public Regex Regex { get; }
    public UserRole Role { get; }
}
