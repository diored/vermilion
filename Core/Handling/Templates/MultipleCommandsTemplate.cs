namespace DioRed.Vermilion.Handling.Templates;

public class MultipleCommandsTemplate : Template
{
    public required string[] Commands { get; init; }

    public static implicit operator MultipleCommandsTemplate(string[] commands) => new() { Commands = commands };

    public override bool Matches(string command)
    {
        return Commands.Contains(command);
    }
}