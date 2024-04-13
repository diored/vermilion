using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Vermilion;

public static class Extensions
{
    public static IServiceCollection AddVermilion(
        this IServiceCollection services,
        Action<BotCoreBuilder> setup
    )
    {
        return services.AddHostedService(serviceProvider =>
        {
            var botCore = BotCore.CreateBuilder(serviceProvider);
            setup?.Invoke(botCore);
            return botCore.Build();
        });
    }
}