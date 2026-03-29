using DioRed.Vermilion.Connectors;
using System.Collections.Immutable;

namespace DioRed.Vermilion.Handling.Context;

/// <summary>
/// Provides chat-related data for a command handling invocation.
/// </summary>
public class ChatContext
{
    /// <summary>
    /// Gets the runtime chat client.
    /// </summary>
    public required ChatClient Client { get; init; }

    /// <summary>
    /// Gets the human-readable chat title when available.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the connector that produced the message.
    /// </summary>
    public required IConnector Connector { get; init; }

    /// <summary>
    /// Gets the chat identity.
    /// </summary>
    public ChatId Id => Client.Metadata.ChatId;

    /// <summary>
    /// Gets the immutable tag set for the chat.
    /// </summary>
    public IImmutableSet<string> Tags => Client.Metadata.Tags;

    /// <summary>
    /// Gets the mutable runtime value bag attached to the chat client.
    /// </summary>
    public Dictionary<string, object?> RuntimeValues => Client.RuntimeValues;

    /// <summary>
    /// Determines whether the chat has the specified tag.
    /// </summary>
    public bool HasTag(string tag)
    {
        return Client.Metadata.HasTag(tag);
    }
}
