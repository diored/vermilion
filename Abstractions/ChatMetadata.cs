using System.Collections.Immutable;

namespace DioRed.Vermilion;

public sealed record ChatMetadata
{
    public required ChatId ChatId { get; init; }
    public ImmutableHashSet<string> Tags { get; init; } = [];

    public bool HasTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return Tags.Contains(tag);
    }

    public ChatMetadata WithTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return this with
        {
            Tags = Tags.Add(tag)
        };
    }

    public ChatMetadata WithTags(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));

        ImmutableHashSet<string> updatedTags = Tags;

        foreach (string tag in tags)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tags));
            updatedTags = updatedTags.Add(tag);
        }

        return this with
        {
            Tags = updatedTags
        };
    }

    public ChatMetadata WithoutTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return this with
        {
            Tags = Tags.Remove(tag)
        };
    }

    public ChatMetadata WithoutTags(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));

        ImmutableHashSet<string> updatedTags = Tags;

        foreach (string tag in tags)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tags));
            updatedTags = updatedTags.Remove(tag);
        }

        return this with
        {
            Tags = updatedTags
        };
    }

    public override string ToString()
    {
        return ChatId.ToString();
    }
}
