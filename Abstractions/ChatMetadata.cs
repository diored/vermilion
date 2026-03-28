using System.Collections.Immutable;

namespace DioRed.Vermilion;

public sealed record ChatMetadata
{
    public required ChatId ChatId { get; init; }
    public ImmutableHashSet<string> Tags { get; init; } = ImmutableHashSet<string>.Empty;

    public override string ToString()
    {
        return ChatId.ToString();
    }
}
