using DioRed.Vermilion.Interaction.Content;

namespace DioRed.Vermilion.Subsystems;

public interface ISubsystem
{
    event EventHandler<MessagePostedEventArgs>? MessagePosted;

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task<PostResult> PostAsync(long internalId, IContent content);
}