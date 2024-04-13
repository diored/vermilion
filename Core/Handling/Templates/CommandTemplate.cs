namespace DioRed.Vermilion.Handling.Templates;

public class CommandTemplate : Template
{
    public required string Command { get; init; }

    public static implicit operator CommandTemplate(string command) => new() { Command = command };

    public override bool Matches(string command)
    {
        return Command == command;
    }
}