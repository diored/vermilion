using DioRed.Vermilion.ChatStorage;

namespace DioRed.Vermilion.Hosting;

/// <summary>
/// Configures the single chat storage used by a Vermilion bot.
/// </summary>
public interface IChatStorageCollection
{
    /// <summary>
    /// Uses a storage instance resolved from the service provider.
    /// </summary>
    void Use(Func<IServiceProvider, IChatStorage> factory);

    /// <summary>
    /// Uses the specified storage instance.
    /// </summary>
    void Use(IChatStorage chatStorage);

    /// <summary>
    /// Resolves and uses a storage instance of the specified type.
    /// </summary>
    void Use<T>() where T : IChatStorage;

    /// <summary>
    /// Uses a storage instance created by the specified factory.
    /// </summary>
    void Use<T>(Func<IServiceProvider, T> factory) where T : IChatStorage;
}
