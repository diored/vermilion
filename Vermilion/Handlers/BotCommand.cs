using System.Text.RegularExpressions;

namespace DioRed.Vermilion.Handlers;

public class BotCommand
{
    public required Regex Regex { get; init; }
    public required UserRole Role { get; init; }
    public required Func<object[]?, Task> Handler { get; init; }
}