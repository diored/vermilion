using DioRed.Vermilion.ChatStorage;
using DioRed.Vermilion.Messages;

namespace DioRed.Vermilion.Hosting;
public class ChatStorageCollection(IServiceProvider services)
{
    internal IChatStorage? ChatStorage { get; private set; }

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

    public void Use(Func<IServiceProvider, IChatStorage> factory)
    {
        Use(factory(services));
    }

    public void Use<T>()
        where T : IChatStorage
    {
        Use(TypeResolver.Resolve<T>(services));
    }

    public void Use<T>(Func<IServiceProvider, T> factory)
        where T : IChatStorage
    {
        Use(factory(services));
    }
}