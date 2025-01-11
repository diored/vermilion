using DioRed.Vermilion.Subsystems;

namespace DioRed.Vermilion.Handling.Context;

public class ChatContext
{
    public required ChatId Id { get; init; }
    public required string Title { get; init; }
    public required ISubsystem Subsystem { get; init; }

    public string[] Tags { get; init; } = [];
    public Dictionary<string, object?> Properties { get; init; } = [];
}