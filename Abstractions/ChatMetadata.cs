using System.Collections.Immutable;

namespace DioRed.Vermilion;

/// <summary>
/// Immutable runtime metadata for a chat known to Vermilion.
/// </summary>
public sealed record ChatMetadata
{
    /// <summary>
    /// Gets the stable chat identity.
    /// </summary>
    public required ChatId ChatId { get; init; }

    /// <summary>
    /// Gets the immutable set of tags assigned to the chat.
    /// </summary>
    public ImmutableHashSet<string> Tags { get; init; } = [];

    /// <summary>
    /// Determines whether the chat has the specified tag.
    /// </summary>
    public bool HasTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return Tags.Contains(tag);
    }

    /// <summary>
    /// Returns a copy of the metadata with the specified tag added.
    /// </summary>
    public ChatMetadata WithTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return this with
        {
            Tags = Tags.Add(tag)
        };
    }

    /// <summary>
    /// Returns a copy of the metadata with all specified tags added.
    /// </summary>
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

    /// <summary>
    /// Returns a copy of the metadata with the specified tag removed.
    /// </summary>
    public ChatMetadata WithoutTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return this with
        {
            Tags = Tags.Remove(tag)
        };
    }

    /// <summary>
    /// Returns a copy of the metadata with all specified tags removed.
    /// </summary>
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

    /// <summary>
    /// Returns a readable representation of the chat identity.
    /// </summary>
    public override string ToString()
    {
        return ChatId.ToString();
    }
}
