using DioRed.Vermilion.Interaction.Content;

namespace DioRed.Vermilion.Connectors;

public interface IConnector
{
    event EventHandler<MessagePostedEventArgs>? MessagePosted;

    string? Version => null;

    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
    Task<PostResult> PostAsync(long internalId, IContent content, CancellationToken ct = default);
    bool IsSuperAdmin(ChatId chatId);
}
