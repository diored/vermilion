using System.Text.RegularExpressions;

namespace DioRed.Vermilion;

public class BotCommand
{
    public Regex Regex { get; init; } = default!;
    public UserRole Role { get; init; }
    public Func<string[]?, Task> Handler { get; init; } = default!;
}