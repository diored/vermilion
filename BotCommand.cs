using System.Text.RegularExpressions;

namespace DioRed.Vermilion;

public class BotCommand
{
    public Regex Regex { get; init; } = default!;
    public bool AdminOnly { get; init; }
    public Func<Task> Handler { get; init; } = default!;
}