using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handlers;

public class BotCommand
{
    public Regex Regex { get; init; } = default!;
    public UserRole Role { get; init; }
    public Func<object[]?, Task> Handler { get; init; } = default!;
}