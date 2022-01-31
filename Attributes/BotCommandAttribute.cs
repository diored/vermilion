using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class BotCommandAttribute : Attribute
{
    public BotCommandAttribute(string pattern, RegexOptions options = RegexOptions.None)
    {
        Pattern = pattern;
        Regex = new Regex(Pattern, options);
    }

    public string Pattern { get; }

    public Regex Regex { get; }
}
