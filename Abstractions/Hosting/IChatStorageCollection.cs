using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Hosting;

public interface IChatStorageCollection
{
    void Use(Func<IServiceProvider, IChatStorage> factory);
    void Use(IChatStorage chatStorage);
    void Use<T>() where T : IChatStorage;
    void Use<T>(Func<IServiceProvider, T> factory) where T : IChatStorage;
}