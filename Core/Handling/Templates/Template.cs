using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handling.Templates;

public abstract class Template
{
    public abstract bool Matches(string command);

    public static implicit operator Template(string command) => new CommandTemplate { Command = command };
    public static implicit operator Template(string[] commands) => new MultipleCommandsTemplate { Commands = commands };
    public static implicit operator Template(Regex regex) => new RegexTemplate { Regex = regex };
}