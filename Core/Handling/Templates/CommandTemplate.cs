namespace DioRed.Vermilion.Handling.Templates;

/// <summary>
/// Matches a single command string.
/// </summary>
public class CommandTemplate : Template
{
    /// <summary>
    /// Gets the command string to match.
    /// </summary>
    public required string Command { get; init; }

    /// <summary>
    /// Converts a command string to a <see cref="CommandTemplate"/>.
    /// </summary>
    public static implicit operator CommandTemplate(string command) => new() { Command = command };

    /// <inheritdoc />
    public override bool Matches(string command)
    {
        return Command == command;
    }
}
