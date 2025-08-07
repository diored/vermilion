using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion.Hosting;
internal static class TypeResolver
{
    public static object Resolve(IServiceProvider services, Type type)
    {
        return services.GetService(type)
            ?? ActivatorUtilities.CreateInstance(services, type);
    }

    public static T Resolve<T>(IServiceProvider services)
    {
        return (T)Resolve(services, typeof(T));
    }
}