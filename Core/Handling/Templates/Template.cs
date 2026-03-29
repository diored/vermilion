using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handling.Templates;

/// <summary>
/// Base class for command matching templates.
/// </summary>
public abstract class Template
{
    /// <summary>
    /// Determines whether the specified command text matches the template.
    /// </summary>
    public abstract bool Matches(string command);

    /// <summary>
    /// Converts a single command string to a command template.
    /// </summary>
    public static implicit operator Template(string command) => new CommandTemplate { Command = command };

    /// <summary>
    /// Converts a set of commands to a multiple-command template.
    /// </summary>
    public static implicit operator Template(string[] commands) => new MultipleCommandsTemplate { Commands = commands };

    /// <summary>
    /// Converts a regular expression to a regex template.
    /// </summary>
    public static implicit operator Template(Regex regex) => new RegexTemplate { Regex = regex };
}
