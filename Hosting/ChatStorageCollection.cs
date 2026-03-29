using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Messages;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Mutable builder used to configure the single chat storage instance.
/// </summary>
public class ChatStorageCollection(IServiceProvider services) : IChatStorageCollection
{
    internal IChatStorage? ChatStorage { get; private set; }

    /// <summary>
    /// Uses the specified chat storage instance.
    /// </summary>
    public void Use(IChatStorage chatStorage)
    {
        if (ChatStorage is not null)
        {
            throw new InvalidOperationException(
                ExceptionMessages.ChatStorageAlreadyInitialized_0
            );
        }

        ChatStorage = chatStorage;
    }

    /// <summary>
    /// Resolves and uses a chat storage instance from the service provider.
    /// </summary>
    public void Use(Func<IServiceProvider, IChatStorage> factory)
    {
        Use(factory(services));
    }

    /// <summary>
    /// Resolves and uses a chat storage instance of the specified type.
    /// </summary>
    public void Use<T>()
        where T : IChatStorage
    {
        Use(TypeResolver.Resolve<T>(services));
    }

    /// <summary>
    /// Uses a chat storage instance created by the specified factory.
    /// </summary>
    public void Use<T>(Func<IServiceProvider, T> factory)
        where T : IChatStorage
    {
        Use(factory(services));
    }
}
