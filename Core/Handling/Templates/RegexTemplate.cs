using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handling.Templates;

public class RegexTemplate : Template
{
    public required Regex Regex { get; init; }

    public static implicit operator RegexTemplate(Regex regex) => new() { Regex = regex };

    public override bool Matches(string command)
    {
        return Regex.IsMatch(command);
    }
}