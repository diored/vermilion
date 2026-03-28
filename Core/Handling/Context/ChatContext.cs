using DioRed.Vermilion.Connectors;
using System.Collections.Immutable;

namespace DioRed.Vermilion.Handling.Context;

public class ChatContext
{
    public required ChatClient Client { get; init; }

    public required string Title { get; init; }
    public required IConnector Connector { get; init; }

    public ChatId Id => Client.Metadata.ChatId;
    public IImmutableSet<string> Tags => Client.Metadata.Tags;
    public Dictionary<string, object?> RuntimeValues => Client.RuntimeValues;
}
