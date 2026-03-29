using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handling.Templates;

/// <summary>
/// Matches commands using a regular expression.
/// </summary>
public class RegexTemplate : Template
{
    /// <summary>
    /// Gets the regular expression used for matching.
    /// </summary>
    public required Regex Regex { get; init; }

    /// <summary>
    /// Converts a regular expression to a <see cref="RegexTemplate"/>.
    /// </summary>
    public static implicit operator RegexTemplate(Regex regex) => new() { Regex = regex };

    /// <inheritdoc />
    public override bool Matches(string command)
    {
        return Regex.IsMatch(command);
    }
}
