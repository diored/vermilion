namespace DioRed.Vermilion.Handling.Templates;

/// <summary>
/// Matches any command from a predefined set.
/// </summary>
public class MultipleCommandsTemplate : Template
{
    /// <summary>
    /// Gets the command strings to match.
    /// </summary>
    public required string[] Commands { get; init; }

    /// <summary>
    /// Converts a command array to a <see cref="MultipleCommandsTemplate"/>.
    /// </summary>
    public static implicit operator MultipleCommandsTemplate(string[] commands) => new() { Commands = commands };

    /// <inheritdoc />
    public override bool Matches(string command)
    {
        return Commands.Contains(command);
    }
}
