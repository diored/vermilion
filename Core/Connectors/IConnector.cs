using DioRed.Vermilion.Interaction.Content;

namespace DioRed.Vermilion.Connectors;

public interface IConnector
{
    event EventHandler<MessagePostedEventArgs>? MessagePosted;

    string? Version => null;

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task<PostResult> PostAsync(long internalId, IContent content);
    bool IsSuperAdmin(ChatId chatId);
}